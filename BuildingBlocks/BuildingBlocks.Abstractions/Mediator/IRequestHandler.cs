using ResulTus;

namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Handles one <typeparamref name="TRequest"/> and produces a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type produced by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The result produced by the handler.</returns>
    Task<Result<TResponse, RequestError>> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Continuation of a request pipeline: invokes the next behavior, or the handler at the end of the chain.
/// </summary>
/// <typeparam name="TResponse">The success payload type.</typeparam>
/// <param name="cancellationToken">Token that cancels the operation.</param>
public delegate Task<Result<TResponse, RequestError>> RequestHandlerDelegate<TResponse>(
    CancellationToken cancellationToken);
