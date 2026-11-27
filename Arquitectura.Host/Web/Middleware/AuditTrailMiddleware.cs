using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Emits a structured audit log entry for every non-infrastructure request.
/// </summary>
/// <param name="next">The next middleware.</param>
/// <param name="logger">The logger.</param>
public sealed class AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
{
    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipAudit(context.Request.Path))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var auditData = new AuditData
        {
            Timestamp = DateTime.UtcNow,
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers.UserAgent.ToString()
        };

        if (context.User.Identity?.IsAuthenticated == true)
        {
            auditData.UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? context.User.FindFirstValue("sub");
            auditData.UserName = context.User.FindFirstValue(ClaimTypes.Name)
                                 ?? context.User.FindFirstValue("email");
            auditData.AuthenticationType = context.User.FindFirstValue("AuthenticationType");
        }

        if (context.User.FindFirstValue("ApiKey") is not null)
        {
            auditData.UserId = context.User.FindFirstValue("ApiKey");
            auditData.AuthenticationType = "ApiKey";
        }

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            auditData.StatusCode = context.Response.StatusCode;
            auditData.DurationMs = stopwatch.ElapsedMilliseconds;
            LogAudit(auditData);
        }
    }

    private void LogAudit(AuditData audit)
    {
        var logLevel = audit.StatusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        logger.Log(
            logLevel,
            "AUDIT: {Method} {Path} -> {StatusCode} ({DurationMs}ms) | User: {UserId} | IP: {ClientIp}",
            audit.Method,
            audit.Path,
            audit.StatusCode,
            audit.DurationMs,
            audit.UserId ?? "anonymous",
            audit.ClientIp);
    }

    private static bool ShouldSkipAudit(PathString path) =>
        path.StartsWithSegments("/health")
        || path.StartsWithSegments("/alive")
        || path.StartsWithSegments("/swagger")
        || path.StartsWithSegments("/hangfire")
        || path.StartsWithSegments("/metrics");
}

/// <summary>Audit record describing one handled request.</summary>
public sealed class AuditData
{
    /// <summary>Gets or sets when the request started (UTC).</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets or sets the HTTP method.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Gets or sets the request path.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets the raw query string.</summary>
    public string? QueryString { get; set; }

    /// <summary>Gets or sets the user identifier.</summary>
    public string? UserId { get; set; }

    /// <summary>Gets or sets the user name.</summary>
    public string? UserName { get; set; }

    /// <summary>Gets or sets the authentication scheme.</summary>
    public string? AuthenticationType { get; set; }

    /// <summary>Gets or sets the client IP address.</summary>
    public string? ClientIp { get; set; }

    /// <summary>Gets or sets the user agent.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets the response status code.</summary>
    public int StatusCode { get; set; }

    /// <summary>Gets or sets the request duration in milliseconds.</summary>
    public long DurationMs { get; set; }
}

/// <summary>Registration helper for <see cref="AuditTrailMiddleware"/>.</summary>
public static class AuditTrailMiddlewareExtensions
{
    /// <summary>Adds <see cref="AuditTrailMiddleware"/> to the pipeline.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder.</returns>
    public static IApplicationBuilder UseAuditTrail(this IApplicationBuilder app) =>
        app.UseMiddleware<AuditTrailMiddleware>();
}
