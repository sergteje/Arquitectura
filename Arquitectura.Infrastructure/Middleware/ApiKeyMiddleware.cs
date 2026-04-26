using System.Security.Cryptography;
using System.Text;

namespace Arquitectura.Infrastructure.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!environment.IsProduction())
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/hangfire") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
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

        var validApiKey = configuration["ApiKey"] ?? "default-dev-key-2026";
        
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
        context.User = new System.Security.Principal.GenericPrincipal(identity, new[] { "ApiUser" });

        await next(context);
    }

    private static bool IsValidApiKey(string submittedKey, string validKey)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(submittedKey),
            Encoding.UTF8.GetBytes(validKey));
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}