using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Adds the standard security headers and marks sensitive endpoint responses as non-cacheable.
/// </summary>
/// <param name="next">The next middleware.</param>
/// <param name="logger">The logger.</param>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
{
    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            await next(context);
            return;
        }

        context.Response.Headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains; preload";
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers.XXSSProtection = "1; mode=block";
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; "
            + "script-src 'self' 'unsafe-inline'; "
            + "style-src 'self' 'unsafe-inline'; "
            + "img-src 'self' data:; "
            + "font-src 'self'; "
            + "connect-src 'self'; "
            + "frame-ancestors 'none';";
        context.Response.Headers.Append(
            "Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), "
            + "magnetometer=(), microphone=(), payment=(), usb=()");

        if (IsSensitivePath(context.Request.Path))
        {
            context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, private";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
        }

        logger.LogDebug("Security headers applied to {Path}", context.Request.Path);
        await next(context);
    }

    private static bool IsSensitivePath(PathString path) =>
        path.StartsWithSegments("/api/auth")
        || path.StartsWithSegments("/api/admin")
        || path.StartsWithSegments("/api/user/profile");
}

/// <summary>Registration helper for <see cref="SecurityHeadersMiddleware"/>.</summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>Adds <see cref="SecurityHeadersMiddleware"/> to the pipeline.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
