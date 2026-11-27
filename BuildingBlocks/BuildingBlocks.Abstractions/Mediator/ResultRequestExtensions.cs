using ResulTus;

namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// Converts the application-owned <see cref="Result{TSuccess}"/> tier into the mediator's
/// <see cref="Result{TSuccess,TError}"/> tier.
/// </summary>
/// <remarks>
/// Handlers compose their business flow with the single-error tier and cross into the mediator
/// contract once, at the end. A validation failure never reaches this conversion: the pipeline
/// behavior short-circuits before the handler runs.
/// </remarks>
public static class ResultRequestExtensions
{
    /// <summary>Projects a domain result into a request result.</summary>
    /// <typeparam name="TSuccess">The success payload type.</typeparam>
    /// <param name="result">The domain result.</param>
    /// <returns>The same outcome expressed as a request result.</returns>
    public static Result<TSuccess, RequestError> ToRequestResult<TSuccess>(this Result<TSuccess> result) =>
        result.Match(
            value => Result<TSuccess, RequestError>.Success(value),
            error => Result<TSuccess, RequestError>.Failure(RequestError.From(error)));

    /// <summary>Projects a unit domain result into a request result.</summary>
    /// <param name="result">The unit domain result.</param>
    /// <returns>The same outcome expressed as a request result with an <see cref="Unit"/> payload.</returns>
    public static Result<Unit, RequestError> ToUnitRequestResult(this Result result) =>
        result.Match(
            () => Result<Unit, RequestError>.Success(Unit.Value),
            error => Result<Unit, RequestError>.Failure(RequestError.From(error)));
}
