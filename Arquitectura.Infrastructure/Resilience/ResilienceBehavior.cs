using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Arquitectura.Infrastructure.Resilience;

public class ResilienceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ResilienceBehavior<TRequest, TResponse>> _logger;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly IAsyncPolicy _circuitBreakerPolicy;

    public ResilienceBehavior(ILogger<ResilienceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} due to {Message}. Waiting {TimeSpan}ms before next retry",
                        retryCount, exception.Message, timeSpan.TotalMilliseconds);
                });

        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for {Duration}s due to: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () => _logger.LogInformation("Circuit breaker reset"),
                onHalfOpen: () => _logger.LogInformation("Circuit breaker half-open, testing..."));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            var policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);

            return await policyWrap.ExecuteAsync(async () =>
            {
                _logger.LogDebug("Executing request {RequestName}", requestName);
                return await next(cancellationToken);
            });
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open for {RequestName}", requestName);
            throw new Exception("Service temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request {RequestName} failed after all retries", requestName);
            throw;
        }
    }
}