using System.Data;
using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Verifies that SQL Server accepts a trivial query.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class SqlServerHealthCheck(IDbConnectionFactory connectionFactory) : IHealthCheck
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
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("SQL Server is responding");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("SQL Server is not responding", exception);
        }
    }
}

/// <summary>
/// Verifies a Redis round-trip through the distributed cache.
/// </summary>
/// <param name="cache">The distributed cache.</param>
public sealed class RedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        const string HealthKey = "__health_check__";

        try
        {
            var expected = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            };

            await cache.SetAsync(HealthKey, expected, options, cancellationToken);
            var actual = await cache.GetAsync(HealthKey, cancellationToken);
            await cache.RemoveAsync(HealthKey, cancellationToken);

            return actual is not null && actual.SequenceEqual(expected)
                ? HealthCheckResult.Healthy("Redis is responding correctly")
                : HealthCheckResult.Unhealthy("Redis read/write verification failed");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis is not responding", exception);
        }
    }
}

/// <summary>
/// Reports whether Hangfire has a configured SQL Server storage.
/// </summary>
/// <param name="databaseOptions">The database options.</param>
/// <param name="logger">The logger.</param>
public sealed class HangfireHealthCheck(
    IOptions<DatabaseOptions> databaseOptions,
    ILogger<HangfireHealthCheck> logger) : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = databaseOptions.Value.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage connection is not configured"));
        }

        logger.LogDebug("Hangfire health check passed");

        return Task.FromResult(HealthCheckResult.Healthy(
            "Hangfire storage is configured",
            new Dictionary<string, object>
            {
                ["Status"] = "Healthy",
                ["Storage"] = "SQL Server"
            }));
    }
}
