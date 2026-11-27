using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Evicts the cache prefixes declared by an <see cref="ICacheInvalidatingRequest"/> after it runs.
/// Requests that do not declare prefixes are untouched, so one module's commands never evict another
/// module's cache entries.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="cache">The distributed cache.</param>
/// <param name="logger">The logger.</param>
public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    IDistributedCacheService cache,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (request is not ICacheInvalidatingRequest invalidatingRequest)
        {
            return response;
        }

        foreach (var prefix in invalidatingRequest.CacheKeyPrefixes)
        {
            await cache.RemoveByPrefixAsync(prefix, cancellationToken);
            logger.LogInformation("Cache invalidated for prefix {CachePrefix}", prefix);
        }

        return response;
    }
}
