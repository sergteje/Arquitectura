namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Marks a query whose successful payload is cached by the caching pipeline behavior.
/// </summary>
public interface ICachableQuery
{
    /// <summary>
    /// Gets the distributed cache key for this query instance.
    /// </summary>
    string GetCacheKey();

    /// <summary>
    /// Gets how long the payload stays cached. Defaults to five minutes.
    /// </summary>
    TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

/// <summary>
/// Marks a command that must invalidate one or more cache key prefixes after it completes successfully.
/// </summary>
public interface ICacheInvalidatingRequest
{
    /// <summary>
    /// Gets the cache key prefixes to evict, for example <c>weatherforecast</c>.
    /// </summary>
    IReadOnlyList<string> CacheKeyPrefixes { get; }
}

/// <summary>
/// Marks an idempotent request that may be retried on transient failures.
/// Non-idempotent commands must not implement this interface.
/// </summary>
public interface IResilientRequest
{
}
