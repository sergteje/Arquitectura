using ResulTus;

namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Dispatches a request to its handler through the registered pipeline behaviors.
/// </summary>
/// <remarks>
/// Every request resolves to a <see cref="Result{TSuccess,TError}"/> whose error is a
/// <see cref="RequestError"/>, so a validation failure travels as data instead of an exception.
/// </remarks>
public interface ISender
{
    /// <summary>
    /// Sends a request through the pipeline.
    /// </summary>
    /// <typeparam name="TResponse">The success payload type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The result produced by the handler.</returns>
    /// <exception cref="InvalidOperationException">No handler is registered for the request type.</exception>
    Task<Result<TResponse, RequestError>> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
