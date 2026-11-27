namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Dispatches a request to its handler through the registered pipeline behaviors.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a request through the pipeline.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The response produced by the handler.</returns>
    /// <exception cref="InvalidOperationException">No handler is registered for the request type.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
