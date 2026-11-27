using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Logs the request and response bodies at <see cref="LogLevel.Debug"/>, with credentials masked.
/// </summary>
/// <remarks>
/// Bodies are a debugging aid, not an audit record: the audit trail already logs method, path,
/// status, duration and user. Keeping the body log at Debug means the default Information level
/// carries one line per request instead of four, and enabling it never writes a password or a
/// token — <see cref="LogBodyRedactor"/> masks those members and omits non-JSON bodies entirely.
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="logger">The logger.</param>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            await next(context);
            return;
        }

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
        logger.LogDebug(
            "HTTP bodies {Method} {Path}{QueryString} -> {StatusCode} ({ElapsedMilliseconds}ms) | Request: {RequestBody} | Response: {ResponseBody}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Response.StatusCode,
            elapsedMs,
            LogBodyRedactor.Redact(requestBody),
            LogBodyRedactor.Redact(responseBody));
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
