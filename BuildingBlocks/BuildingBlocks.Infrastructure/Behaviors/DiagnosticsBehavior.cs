using System.Diagnostics;
using BuildingBlocks.Abstractions.Mediator;
using ResulTus;
using ResulTus.Diagnostics;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Publishes one span and one metric measurement per request through
/// <c>ResulTus.Diagnostics</c>.
/// </summary>
/// <remarks>
/// <para>
/// The span lives on <see cref="ResultDiagnostics"/>'s activity source and its tags use the same
/// names <c>ResulTus.Diagnostics</c> publishes, so a collector reads both without a second query.
/// The measurement goes through <see cref="ResultMetrics.Record"/>, the entry point that package
/// documents for a caller who already holds a result.
/// </para>
/// <para>
/// The package's <c>Measure</c>/<c>MeasureAsync</c> helpers do not cover
/// <see cref="Result{TSuccess,TError}"/> — it only overloads the single-error tier — so this
/// behavior composes the two primitives the helper composes internally instead of wrapping the
/// operation twice.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type.</typeparam>
public sealed class DiagnosticsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const string RequestTag = "mediator.request";
    private const string SuccessTag = "resul_tus.success";
    private const string ErrorTypeTag = "resul_tus.error_type";
    private const string ErrorCodeTag = "resul_tus.error_code";

    /// <inheritdoc />
    public async Task<Result<TResponse, RequestError>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = ResultDiagnostics.ActivitySource.StartActivity(
            $"mediator {requestName}",
            ActivityKind.Internal);

        activity?.SetTag(RequestTag, requestName);

        var stopwatch = Stopwatch.StartNew();
        var result = await next(cancellationToken);
        stopwatch.Stop();

        result.TryGetError(out var requestError);
        var isSuccess = requestError is null;
        var error = requestError?.Error ?? Error.None;

        if (isSuccess)
        {
            activity?.SetTag(SuccessTag, true);
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error, error.Message);
            activity?.SetTag(SuccessTag, false);
            activity?.SetTag(ErrorTypeTag, error.Type.ToString());
            activity?.SetTag(ErrorCodeTag, error.Code);
        }

        ResultMetrics.Record(isSuccess, error, stopwatch.Elapsed);

        return result;
    }
}
