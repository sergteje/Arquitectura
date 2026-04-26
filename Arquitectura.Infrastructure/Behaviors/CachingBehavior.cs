using Arquitectura.Application.Interfaces;
using Arquitectura.Infrastructure.Cache;
using MediatR;

namespace Arquitectura.Infrastructure.Behaviors;

public class CachedResult<T> { public T? Value { get; set; } public bool IsSuccess { get; set; } public Domain.Models.ResultError? Error { get; set; } }

public class CachingBehavior<TRequest, TResponse>(IDistributedCacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (request is not ICachableQuery cachableRequest)
        {
            return await next(cancellationToken);
        }

        var cacheKey = cachableRequest.GetCacheKey();
        var cachedResult = await cache.GetAsync<CachedResult<object>>(cacheKey, cancellationToken);

        if (cachedResult != null && cachedResult.IsSuccess && cachedResult.Value != null)
        {
            logger.LogInformation("CachingBehavior: Cache HIT for '{CacheKey}'", cacheKey);
            return await next(cancellationToken);
        }

        logger.LogInformation("CachingBehavior: Cache MISS for '{CacheKey}'", cacheKey);
        var response = await next(cancellationToken);

        if (response != null)
        {
            try
            {
                var resultToCache = new CachedResult<object> { Value = response, IsSuccess = true };
                await cache.SetAsync(cacheKey, resultToCache, TimeSpan.FromMinutes(5), cancellationToken);
            }
            catch (Exception ex) { logger.LogWarning(ex, "CachingBehavior: Failed to cache '{CacheKey}'", cacheKey); }
        }

        return response;
    }
}