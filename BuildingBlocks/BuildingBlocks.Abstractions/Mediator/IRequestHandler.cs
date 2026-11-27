namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Handles one <typeparamref name="TRequest"/> and produces a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The response produced by the handler.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Continuation of a request pipeline: invokes the next behavior, or the handler at the end of the chain.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="cancellationToken">Token that cancels the operation.</param>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);
