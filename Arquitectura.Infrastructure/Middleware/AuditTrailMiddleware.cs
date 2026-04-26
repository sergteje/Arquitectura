using System.Diagnostics;
using System.Security.Claims;

namespace Arquitectura.Infrastructure.Middleware;

public class AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
{
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

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            auditData.UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            auditData.UserName = context.User.FindFirstValue(ClaimTypes.Name);
            auditData.AuthenticationType = context.User.FindFirstValue("AuthenticationType");
        }

        if (context.User?.FindFirstValue("ApiKey") != null)
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

    private static bool ShouldSkipAudit(PathString path)
    {
        return path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/swagger") ||
               path.StartsWithSegments("/hangfire") ||
               path.StartsWithSegments("/metrics");
    }
}

public class AuditData
{
    public DateTime Timestamp { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? QueryString { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? AuthenticationType { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
}

public static class AuditTrailMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditTrail(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditTrailMiddleware>();
    }
}