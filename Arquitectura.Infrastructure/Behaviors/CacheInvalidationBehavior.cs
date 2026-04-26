using Arquitectura.Application.Interfaces;
using Arquitectura.Infrastructure.Cache;
using MediatR;

namespace Arquitectura.Infrastructure.Behaviors;

public class CacheInvalidationBehavior<TRequest, TResponse>(IDistributedCacheService cache, ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (request is not ICachableQuery)
        {
            await cache.RemoveByPrefixAsync("weatherforecast", cancellationToken);
            logger.LogInformation("Cache invalidated for weather forecasts");
        }

        return response;
    }
}