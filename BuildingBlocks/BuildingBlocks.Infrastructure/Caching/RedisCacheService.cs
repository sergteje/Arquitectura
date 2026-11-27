using System.Text;
using BuildingBlocks.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Redis-backed <see cref="IDistributedCacheService"/> built directly on the shared
/// <see cref="IConnectionMultiplexer"/>. Every failure is logged and swallowed: an unavailable cache
/// degrades performance, never correctness.
/// </summary>
/// <param name="redis">The shared Redis multiplexer.</param>
/// <param name="logger">The logger.</param>
public sealed class RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : IDistributedCacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await redis.GetDatabase().StringGetAsync(key);

            return value.HasValue
                ? JsonConvert.DeserializeObject<T>(value.ToString())
                : default;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error reading cache key {CacheKey}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(value);
            await redis.GetDatabase().StringSetAsync(key, payload, expiration ?? DefaultExpiration);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error writing cache key {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await redis.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error removing cache key {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();

            foreach (var server in redis.GetServers())
            {
                if (server.IsReplica)
                {
                    continue;
                }

                var keys = server.Keys(pattern: $"{keyPrefix}*").ToArray();
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error removing cache keys by prefix {CachePrefix}", keyPrefix);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await redis.GetDatabase().KeyExistsAsync(key);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error checking cache key {CacheKey}", key);
            return false;
        }
    }
}
