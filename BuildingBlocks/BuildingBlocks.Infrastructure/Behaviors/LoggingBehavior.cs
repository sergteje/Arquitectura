using System.Diagnostics;
using BuildingBlocks.Abstractions.Mediator;
using Microsoft.Extensions.Logging;
using ResulTus;
using ResulTus.Logging;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Logs every request with its name, duration and outcome.
/// </summary>
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

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            // ResulTus.Logging covers the non-generic tier, so the domain error is projected into a
            // Result just for logging; the validation payload stays in the original result.
            var outcome = response.TryGetError(out var requestError) && requestError is not null
                ? Result.Failure(requestError.Error)
                : Result.Success();

            _ = outcome.LogAll(
                logger,
                error => error.Type is ErrorType.Unauthorized
                    or ErrorType.Forbidden
                    or ErrorType.NotFound
                    or ErrorType.Validation
                    ? LogLevel.Information
                    : LogLevel.Error);

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
