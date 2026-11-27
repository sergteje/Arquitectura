using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Requires an <c>X-API-Key</c> header in production. Development requests bypass the check.
/// </summary>
/// <remarks>
/// The expected key is read from the <c>ApiKey:Key</c> configuration value; there is no hardcoded
/// fallback. When the key is missing in production the middleware fails closed.
/// </remarks>
/// <param name="next">The next middleware.</param>
/// <param name="configuration">The application configuration.</param>
/// <param name="environment">The hosting environment.</param>
/// <param name="logger">The logger.</param>
public sealed class ApiKeyMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<ApiKeyMiddleware> logger)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    /// <summary>Runs the middleware.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the request finishes.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!environment.IsProduction() || IsExempt(context.Request.Path))
        {
            await next(context);
            return;
        }

        var validApiKey = configuration["ApiKey:Key"];

        if (string.IsNullOrWhiteSpace(validApiKey))
        {
            logger.LogError("ApiKey:Key is not configured; rejecting request in production");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API Key not configured",
                message = "The server is not configured to authenticate API keys"
            });
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API Key not provided",
                message = $"Please include '{ApiKeyHeaderName}' header with a valid API key"
            });
            return;
        }

        if (!IsValidApiKey(extractedApiKey!, validApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API Key",
                message = "The provided API key is not valid"
            });
            return;
        }

        var claims = new[]
        {
            new System.Security.Claims.Claim("ApiKey", extractedApiKey!),
            new System.Security.Claims.Claim("AuthenticationType", "ApiKey")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "ApiKey");
        context.User = new System.Security.Principal.GenericPrincipal(identity, ["ApiUser"]);

        await next(context);
    }

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/swagger")
        || path.StartsWithSegments("/scalar")
        || path.StartsWithSegments("/openapi")
        || path.StartsWithSegments("/hangfire")
        || path.StartsWithSegments("/health")
        || path.StartsWithSegments("/alive")
        || path.StartsWithSegments("/metrics");

    private static bool IsValidApiKey(string submittedKey, string validKey) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(submittedKey),
            Encoding.UTF8.GetBytes(validKey));
}

/// <summary>Registration helper for <see cref="ApiKeyMiddleware"/>.</summary>
public static class ApiKeyMiddlewareExtensions
{
    /// <summary>Adds <see cref="ApiKeyMiddleware"/> to the pipeline.</summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder.</returns>
    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app) =>
        app.UseMiddleware<ApiKeyMiddleware>();
}
