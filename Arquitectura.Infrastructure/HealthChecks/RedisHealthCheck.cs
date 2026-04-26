using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arquitectura.Infrastructure.HealthChecks;

public class RedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthKey = "__health_check__";
            var testData = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));

            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) };
            await cache.SetAsync(healthKey, testData, options, cancellationToken);

            var retrieved = await cache.GetAsync(healthKey, cancellationToken);
            await cache.RemoveAsync(healthKey, cancellationToken);

            if (retrieved == null || !retrieved.SequenceEqual(testData))
            {
                return HealthCheckResult.Unhealthy("Redis read/write verification failed");
            }

            return HealthCheckResult.Healthy("Redis is responding correctly");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is not responding", ex);
        }
    }
}