namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// A cross-cutting step wrapped around a request handler. Behaviors run in registration order:
/// the first registered behavior is the outermost one.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Runs the behavior and either continues the pipeline through <paramref name="next"/> or short-circuits it.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="next">Continuation of the pipeline.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The response produced by the pipeline.</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
