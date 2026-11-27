using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Propagates an <c>X-Correlation-Id</c> header (creating one when absent), exposes it on the
/// request items, and pushes it into a log scope together with the current trace identifiers.
/// </summary>
/// <remarks>
/// The scope is what makes the header useful: without it the identifier exists on the response and
/// nowhere in the logs, so a client reporting it cannot be traced back to anything.
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="logger">The logger whose scope carries the identifiers.</param>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = correlationId,
                   ["TraceId"] = Activity.Current?.TraceId.ToString(),
                   ["SpanId"] = Activity.Current?.SpanId.ToString()
               }))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}

/// <summary>Registration helper for <see cref="CorrelationIdMiddleware"/>.</summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>Adds <see cref="CorrelationIdMiddleware"/> to the pipeline.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder.</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
