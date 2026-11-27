namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Propagates an <c>X-Correlation-Id</c> header (creating one when absent) and exposes it on the
/// request items so logs can be correlated across hops.
/// </summary>
/// <param name="next">The next middleware.</param>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
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

        await next(context);
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
