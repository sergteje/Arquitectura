using System.Text;
using BuildingBlocks.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Redis-backed <see cref="IDistributedCacheService"/>. Every failure is logged and swallowed: an
/// unavailable cache degrades performance, never correctness.
/// </summary>
/// <param name="distributedCache">The distributed cache.</param>
/// <param name="redis">The multiplexer used for prefix eviction.</param>
/// <param name="logger">The logger.</param>
public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : IDistributedCacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await distributedCache.GetAsync(key, cancellationToken);
            if (payload is null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(payload));
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
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            await distributedCache.SetAsync(key, payload, options, cancellationToken);
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
            await distributedCache.RemoveAsync(key, cancellationToken);
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

                var keys = server
                    .Keys(pattern: $"{keyPrefix}*")
                    .ToArray();

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
        var payload = await distributedCache.GetAsync(key, cancellationToken);
        return payload is not null;
    }
}
