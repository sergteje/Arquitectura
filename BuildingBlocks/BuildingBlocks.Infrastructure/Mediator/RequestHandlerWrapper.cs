using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.DependencyInjection;
using ResulTus;

namespace BuildingBlocks.Infrastructure.Mediator;

/// <summary>
/// Non-generic handle used by <see cref="Mediator"/> to reach a closed wrapper instance.
/// </summary>
/// <typeparam name="TResponse">The success payload type.</typeparam>
public abstract class RequestHandlerWrapper<TResponse>
{
    /// <summary>
    /// Runs the request through the handler and the registered behaviors.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The result produced by the pipeline.</returns>
    public abstract Task<Result<TResponse, RequestError>> Handle(
        IRequest<TResponse> request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Builds the behavior chain for one request/response pair and invokes the handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type.</typeparam>
/// <param name="serviceProvider">The scoped service provider.</param>
public sealed class RequestHandlerWrapper<TRequest, TResponse>(IServiceProvider serviceProvider)
    : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override Task<Result<TResponse, RequestError>> Handle(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> pipeline = ct => handler.Handle(typedRequest, ct);

        foreach (var behavior in serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var next = pipeline;
            pipeline = ct => behavior.Handle(typedRequest, next, ct);
        }

        return pipeline(cancellationToken);
    }
}
