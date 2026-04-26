using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace Arquitectura.Infrastructure.HealthChecks;

public class DatabaseHealthCheck(IDbConnection connection) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM dbo.WeatherForecasts";
            var result = await Task.Run(() => command.ExecuteScalar(), cancellationToken);

            if (result != null)
            {
                return HealthCheckResult.Healthy("Database connection is healthy", new Dictionary<string, object>
                {
                    ["recordCount"] = result
                });
            }
            return HealthCheckResult.Degraded("Database connection is degraded");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}