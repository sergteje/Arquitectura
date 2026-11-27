using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Retries and circuit-breaks requests that explicitly opt in through <see cref="IResilientRequest"/>.
/// Non-idempotent commands must not opt in: retrying a write can duplicate its effect.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ResilienceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ResilienceBehavior<TRequest, TResponse>> _logger;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly IAsyncPolicy _circuitBreakerPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceBehavior{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ResilienceBehavior(ILogger<ResilienceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>(exception => exception is not OperationCanceledException)
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
                onRetry: (exception, delay, retryCount, _) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} for {RequestName} in {DelayMilliseconds}ms due to {Message}",
                        retryCount,
                        typeof(TRequest).Name,
                        delay.TotalMilliseconds,
                        exception.Message);
                });

        _circuitBreakerPolicy = Policy
            .Handle<Exception>(exception => exception is not OperationCanceledException)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(
                        "Circuit breaker opened for {BreakSeconds}s due to {Message}",
                        duration.TotalSeconds,
                        exception.Message);
                },
                onReset: () => _logger.LogInformation("Circuit breaker reset"),
                onHalfOpen: () => _logger.LogInformation("Circuit breaker half-open"));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IResilientRequest)
        {
            return await next(cancellationToken);
        }

        var policy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);

        try
        {
            return await policy.ExecuteAsync(_ => next(cancellationToken), new Context());
        }
        catch (BrokenCircuitException exception)
        {
            _logger.LogError(exception, "Circuit breaker is open for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
