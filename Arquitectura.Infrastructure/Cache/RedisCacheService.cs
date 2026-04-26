using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Arquitectura.Infrastructure.Cache;

public interface IDistributedCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string keyPrefix, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}

public class RedisCacheService(IDistributedCache distributedCache, IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : IDistributedCacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await distributedCache.GetAsync(key, ct);
            if (value == null) return default;
            var jsonString = System.Text.Encoding.UTF8.GetString(value);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error getting from Redis cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            options.SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(5));
            options.SetSlidingExpiration(TimeSpan.FromMinutes(2));
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            await distributedCache.SetAsync(key, System.Text.Encoding.UTF8.GetBytes(jsonString), options, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error setting Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await distributedCache.RemoveAsync(key, ct); }
        catch (Exception ex) { logger.LogWarning(ex, "Error removing from Redis cache for key: {Key}", key); }
    }

    public async Task RemoveByPrefixAsync(string keyPrefix, CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var server = redis.GetServers().First();
            var keys = server.Keys(pattern: $"{keyPrefix}*").ToArray();
            if (keys.Length > 0) await db.KeyDeleteAsync(keys);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Error removing keys by prefix: {Prefix}", keyPrefix); }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var value = await distributedCache.GetAsync(key, ct);
        return value != null;
    }
}