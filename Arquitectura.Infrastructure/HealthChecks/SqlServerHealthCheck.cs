using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace Arquitectura.Infrastructure.HealthChecks;

public class SqlServerHealthCheck(IDbConnection connection) : IHealthCheck
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
            command.CommandText = "SELECT 1";
            await Task.Run(() => command.ExecuteScalar(), cancellationToken);

            return HealthCheckResult.Healthy("SQL Server is responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server is not responding", ex);
        }
    }
}