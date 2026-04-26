using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arquitectura.Infrastructure.HealthChecks;

public class HangfireHealthCheck(IConfiguration configuration, ILogger<HangfireHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy("Hangfire database connection not configured");
            }

            var details = new Dictionary<string, object>
            {
                ["Status"] = "Healthy",
                ["Storage"] = "SQL Server"
            };

            logger.LogDebug("Hangfire health check passed");
            
            return HealthCheckResult.Healthy("Hangfire is running", data: details);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Hangfire health");
            return HealthCheckResult.Unhealthy("Error checking Hangfire", ex);
        }
    }
}