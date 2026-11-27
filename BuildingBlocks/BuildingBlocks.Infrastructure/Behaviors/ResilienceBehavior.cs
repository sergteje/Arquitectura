using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ResulTus;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Retries requests that explicitly opt in through <see cref="IResilientRequest"/>.
/// Non-idempotent commands must not opt in: retrying a write can duplicate its effect.
/// </summary>
/// <remarks>
/// A failed result is the normal outcome of the pipeline, so the policy inspects the result instead of
/// waiting for an exception: only infrastructure categories (<see cref="ErrorType.Failure"/>,
/// <see cref="ErrorType.Timeout"/> and <see cref="ErrorType.Unexpected"/>) are retried. A validation,
/// authentication or not-found failure short-circuits immediately.
/// </remarks>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type.</typeparam>
public sealed class ResilienceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int MaxRetryAttempts = 2;

    private readonly ILogger<ResilienceBehavior<TRequest, TResponse>> _logger;
    private readonly AsyncRetryPolicy<Result<TResponse, RequestError>> _retryPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceBehavior{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ResilienceBehavior(ILogger<ResilienceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;

        _retryPolicy = Policy<Result<TResponse, RequestError>>
            .HandleResult(IsTransient)
            .WaitAndRetryAsync(
                retryCount: MaxRetryAttempts,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
                onRetry: (outcome, delay, retryCount, _) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} for {RequestName} in {DelayMilliseconds}ms due to {ErrorCode}",
                        retryCount,
                        typeof(TRequest).Name,
                        delay.TotalMilliseconds,
                        Describe(outcome.Result));
                });
    }

    /// <inheritdoc />
    public async Task<Result<TResponse, RequestError>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IResilientRequest)
        {
            return await next(cancellationToken);
        }

        var result = await _retryPolicy.ExecuteAsync(_ => next(cancellationToken), new Context());

        if (IsTransient(result))
        {
            _logger.LogError(
                "Retries exhausted for {RequestName} after {MaxRetryAttempts} attempts due to {ErrorCode}",
                typeof(TRequest).Name,
                MaxRetryAttempts,
                Describe(result));
        }

        return result;
    }

    private static bool IsTransient(Result<TResponse, RequestError> result) =>
        result.TryGetError(out var requestError)
        && requestError is not null
        && requestError.Error.Type is ErrorType.Failure or ErrorType.Timeout or ErrorType.Unexpected;

    private static string Describe(Result<TResponse, RequestError> result) =>
        result.TryGetError(out var requestError) && requestError is not null
            ? requestError.Error.Code
            : "none";
}
