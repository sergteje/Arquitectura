namespace BuildingBlocks.Abstractions.Caching;

/// <summary>
/// Distributed cache used by modules for query payloads. Implementations must never throw on
/// transient cache failures: a broken cache degrades performance, never correctness.
/// </summary>
public interface IDistributedCacheService
{
    /// <summary>
    /// Reads and deserializes a cached value.
    /// </summary>
    /// <typeparam name="T">The cached payload type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The cached value, or <c>null</c> on a miss or a cache failure.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes and stores a value.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expiration">Absolute expiration. Defaults to five minutes.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a single key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes every key that starts with <paramref name="keyPrefix"/>.
    /// </summary>
    /// <param name="keyPrefix">The key prefix to evict.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a key exists.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns><c>true</c> when the key is present.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis connection configuration, bound from the <c>Redis</c> configuration section.
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Gets or sets the StackExchange.Redis connection string.
    /// </summary>
    public string Connection { get; set; } = "localhost:6379";
}
