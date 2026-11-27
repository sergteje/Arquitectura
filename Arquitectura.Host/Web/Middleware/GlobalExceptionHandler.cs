using BuildingBlocks.Infrastructure.Behaviors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResulTus;
using ResulTus.AspNetCore;
using ResulTus.Validation;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 problem details with the legacy status/title mapping.
/// Structured validation failures are rendered through <c>ResulTus.AspNetCore</c>.
/// </summary>
/// <param name="logger">The logger.</param>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is RequestValidationException requestValidation)
        {
            logger.LogWarning("Request validation failed: {Message}", exception.Message);

            var validationResult = Result<object, ValidationErrors>
                .Failure(requestValidation.Errors)
                .ToHttpResult();

            await validationResult.ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(exception, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, string traceId)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = exception switch
        {
            ArgumentException => "Bad Request",
            BadHttpRequestException => "Bad Request",
            KeyNotFoundException => "Not Found",
            UnauthorizedAccessException => "Unauthorized",
            InvalidOperationException => "Conflict",
            ValidationException => "Validation Error",
            _ => "Internal Server Error"
        };

        var detail = exception switch
        {
            ValidationException validationException =>
                string.Join("; ", validationException.Errors.Select(error => error.ErrorMessage)),
            _ => exception.Message
        };

        return new ProblemDetails
        {
            Type = $"https://tools.ietf.org/html/rfc7231#section-6.{statusCode / 100 - 4}.{(statusCode % 100) / 10}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Extensions = { ["code"] = GetErrorCode(statusCode), ["traceId"] = traceId }
        };
    }

    private static string GetErrorCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "BAD_REQUEST",
        StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
        StatusCodes.Status403Forbidden => "FORBIDDEN",
        StatusCodes.Status404NotFound => "NOT_FOUND",
        StatusCodes.Status409Conflict => "CONFLICT",
        StatusCodes.Status503ServiceUnavailable => "SERVICE_UNAVAILABLE",
        _ => "INTERNAL_ERROR"
    };
}
