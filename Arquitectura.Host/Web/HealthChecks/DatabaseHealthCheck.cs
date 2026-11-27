using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arquitectura.Host.Web.HealthChecks;

/// <summary>
/// Verifies that the database is reachable and the primary table is queryable.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class DatabaseHealthCheck(IDbConnectionFactory connectionFactory) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM dbo.WeatherForecasts";
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result is not null
                ? HealthCheckResult.Healthy("Database connection is healthy", new Dictionary<string, object>
                {
                    ["recordCount"] = result
                })
                : HealthCheckResult.Degraded("Database connection is degraded");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", exception);
        }
    }
}
