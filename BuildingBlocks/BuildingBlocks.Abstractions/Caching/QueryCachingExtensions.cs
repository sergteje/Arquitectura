using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Mediator;
using ResulTus;

namespace BuildingBlocks.Abstractions.Caching;

/// <summary>
/// Read-through caching for query handlers.
/// </summary>
/// <remarks>
/// Caching is explicit rather than a pipeline behavior because <see cref="Result{T}"/> is a readonly
/// struct without public setters and therefore does not round-trip through a serializer. Only the
/// successful payload is cached, which is also the only part worth caching.
/// </remarks>
public static class QueryCachingExtensions
{
    /// <summary>
    /// Returns the cached payload for <paramref name="query"/>, or runs <paramref name="factory"/> and
    /// caches its successful payload.
    /// </summary>
    /// <typeparam name="T">The cached payload type.</typeparam>
    /// <param name="cache">The distributed cache.</param>
    /// <param name="query">The query that owns the cache key and duration.</param>
    /// <param name="factory">The handler logic that produces the payload.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The cached or freshly produced result.</returns>
    public static async Task<Result<T>> GetOrSetAsync<T>(
        this IDistributedCacheService cache,
        ICachableQuery query,
        Func<CancellationToken, Task<Result<T>>> factory,
        CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(factory);

        var cacheKey = query.GetCacheKey();

        var cached = await cache.GetAsync<T>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<T>.Success(cached);
        }

        var result = await factory(cancellationToken);

        if (result.TryGetValue(out var value) && value is not null)
        {
            await cache.SetAsync(cacheKey, value, query.CacheDuration, cancellationToken);
        }

        return result;
    }
}
