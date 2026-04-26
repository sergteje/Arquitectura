using System.Diagnostics;
using System.Text;

namespace Arquitectura.Infrastructure.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = string.Empty;
        var responseBody = string.Empty;

        if (context.Request.Method != "GET" && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var originalResponseBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
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
            responseBody = await responseReader.ReadToEndAsync();

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

        if (context.Response.StatusCode >= 500)
        {
            logger.LogError(logMessage.ToString());
        }
        else if (context.Response.StatusCode >= 400)
        {
            logger.LogWarning(logMessage.ToString());
        }
        else
        {
            logger.LogInformation(logMessage.ToString());
        }
    }

    private static string TruncateForLogging(string content)
    {
        const int maxLength = 1000;
        if (string.IsNullOrEmpty(content)) return string.Empty;
        
        if (content.Length <= maxLength) return content;
        return content[..maxLength] + "... [truncated]";
    }
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}