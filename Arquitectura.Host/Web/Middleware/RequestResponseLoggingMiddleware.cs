using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Logs request and response bodies (truncated) at the level implied by the status code.
/// </summary>
/// <param name="next">The next middleware.</param>
/// <param name="logger">The logger.</param>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int MaxBodyLength = 1000;

    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = string.Empty;

        if (context.Request.Method != "GET" && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var originalResponseBody = context.Response.Body;
        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            memoryStream.Position = 0;
            using var responseReader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
            var responseBody = await responseReader.ReadToEndAsync();

            LogRequestResponse(context, requestBody, responseBody, stopwatch.ElapsedMilliseconds);

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private void LogRequestResponse(HttpContext context, string requestBody, string responseBody, long elapsedMs)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendLine("=== Request/Response Log ===");
        logMessage.AppendLine($"Method: {context.Request.Method}");
        logMessage.AppendLine($"Path: {context.Request.Path}{context.Request.QueryString}");
        logMessage.AppendLine($"Status: {context.Response.StatusCode}");
        logMessage.AppendLine($"Duration: {elapsedMs}ms");

        if (!string.IsNullOrEmpty(requestBody))
        {
            logMessage.AppendLine($"Request Body: {TruncateForLogging(requestBody)}");
        }

        if (!string.IsNullOrEmpty(responseBody))
        {
            logMessage.AppendLine($"Response Body: {TruncateForLogging(responseBody)}");
        }

        logMessage.AppendLine("============================");

        var message = logMessage.ToString();

        if (context.Response.StatusCode >= 500)
        {
            logger.LogError("{Log}", message);
        }
        else if (context.Response.StatusCode >= 400)
        {
            logger.LogWarning("{Log}", message);
        }
        else
        {
            logger.LogInformation("{Log}", message);
        }
    }

    private static string TruncateForLogging(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        return content.Length <= MaxBodyLength ? content : content[..MaxBodyLength] + "... [truncated]";
    }
}

/// <summary>Registration helper for <see cref="RequestResponseLoggingMiddleware"/>.</summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    /// <summary>Adds <see cref="RequestResponseLoggingMiddleware"/> to the pipeline.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder.</returns>
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
}
