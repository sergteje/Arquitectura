using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Mediator;

/// <summary>
/// Default <see cref="ISender"/> implementation: resolves the closed request handler wrapper for the
/// runtime request type and lets it compose the pipeline behaviors.
/// </summary>
/// <param name="serviceProvider">The scoped service provider.</param>
public sealed class Mediator(IServiceProvider serviceProvider) : ISender
{
    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapperType = typeof(RequestHandlerWrapper<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var wrapper = (RequestHandlerWrapper<TResponse>)serviceProvider.GetRequiredService(wrapperType);

        return wrapper.Handle(request, cancellationToken);
    }
}
