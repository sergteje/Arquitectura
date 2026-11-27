namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// A request that produces a <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response produced by the handler.</typeparam>
public interface IRequest<TResponse>
{
}

/// <summary>
/// A request that changes state. Semantically a command, structurally an <see cref="IRequest{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The response produced by the handler.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// A request that only reads state. Semantically a query, structurally an <see cref="IRequest{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The response produced by the handler.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
