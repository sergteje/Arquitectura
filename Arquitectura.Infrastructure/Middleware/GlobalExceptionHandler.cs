using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace Arquitectura.Infrastructure.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(exception, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, string traceId)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status409Conflict,
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = exception switch
        {
            ArgumentException => "Bad Request",
            KeyNotFoundException => "Not Found",
            UnauthorizedAccessException => "Unauthorized",
            InvalidOperationException => "Conflict",
            FluentValidation.ValidationException => "Validation Error",
            _ => "Internal Server Error"
        };

        var detail = exception switch
        {
            FluentValidation.ValidationException validationEx =>
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
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