using System.Diagnostics;
using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.Logging;
using ResulTus;
using ResulTus.Logging;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Logs the outcome of every request.
/// </summary>
/// <remarks>
/// A successful request leaves one line at Information — the audit trail's. Its timing lives in the
/// mediator span and the duration histogram, so the handler detail stays at Debug. A failed request
/// adds exactly one line with its error code and a level derived from the error type.
/// </remarks>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type.</typeparam>
/// <param name="logger">The logger.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<Result<TResponse, RequestError>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogDebug("Handling {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (response.TryGetError(out var requestError) && requestError is not null)
            {
                // ResulTus.Logging covers the non-generic tier, so the domain error is projected
                // into a Result just for the outcome line. The validation payload stays in the
                // original result and is reported by ValidationBehavior, which owns it.
                var outcome = Result.Failure(requestError.Error);

                _ = outcome.LogAll(
                    logger,
                    error => error.Type is ErrorType.Unauthorized
                        or ErrorType.Forbidden
                        or ErrorType.NotFound
                        or ErrorType.Validation
                        ? LogLevel.Information
                        : LogLevel.Error);
            }
            else
            {
                logger.LogDebug(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Cancelled {RequestName}", requestName);
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "Failed {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
