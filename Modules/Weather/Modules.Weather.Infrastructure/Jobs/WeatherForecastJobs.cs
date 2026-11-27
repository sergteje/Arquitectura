using Hangfire;
using Microsoft.Extensions.Logging;

namespace Modules.Weather.Infrastructure.Jobs;

/// <summary>
/// Recurring maintenance jobs owned by the Weather module.
/// </summary>
/// <param name="logger">The logger.</param>
public sealed class WeatherForecastJobs(ILogger<WeatherForecastJobs> logger)
{
    /// <summary>Removes forecasts older than the retention window.</summary>
    /// <returns>A task that completes when the job finishes.</returns>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public async Task CleanupOldForecasts()
    {
        logger.LogInformation("Starting cleanup of old weather forecasts");
        await Task.Delay(1000);
        logger.LogInformation("Cleanup of old weather forecasts completed");
    }

    /// <summary>Generates the daily weather report.</summary>
    /// <returns>A task that completes when the job finishes.</returns>
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task GenerateDailyReport()
    {
        logger.LogInformation("Generating daily weather report");
        await Task.Delay(2000);
        logger.LogInformation("Daily weather report generated");
    }

    /// <summary>Checks for extreme weather conditions.</summary>
    /// <returns>A task that completes when the job finishes.</returns>
    public async Task CheckForExtremeWeather()
    {
        logger.LogInformation("Checking for extreme weather conditions");
        await Task.Delay(500);
        logger.LogInformation("Extreme weather check completed");
    }

    /// <summary>Synchronizes external weather data.</summary>
    /// <returns>A task that completes when the job finishes.</returns>
    public async Task SyncExternalWeatherData()
    {
        logger.LogInformation("Synchronizing external weather data");
        await Task.Delay(1500);
        logger.LogInformation("External weather data synchronization completed");
    }
}

/// <summary>
/// Schedules the Weather module's recurring jobs.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Schedules the forecast cleanup job.</summary>
    void ScheduleCleanupOldForecasts();

    /// <summary>Schedules the daily report job.</summary>
    void ScheduleDailyReport();

    /// <summary>Schedules the extreme-weather check job.</summary>
    void ScheduleWeatherCheck();

    /// <summary>Schedules the external-data synchronization job.</summary>
    void ScheduleExternalSync();
}

/// <summary>
/// Hangfire-backed <see cref="IJobScheduler"/>.
/// </summary>
/// <param name="recurringJobManager">The Hangfire recurring job manager.</param>
public sealed class HangfireJobScheduler(IRecurringJobManager recurringJobManager) : IJobScheduler
{
    /// <inheritdoc />
    public void ScheduleCleanupOldForecasts() => recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
        "cleanup-old-forecasts",
        job => job.CleanupOldForecasts(),
        "0 2 * * *");

    /// <inheritdoc />
    public void ScheduleDailyReport() => recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
        "daily-report",
        job => job.GenerateDailyReport(),
        "0 8 * * *");

    /// <inheritdoc />
    public void ScheduleWeatherCheck() => recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
        "weather-check",
        job => job.CheckForExtremeWeather(),
        "*/30 * * * *");

    /// <inheritdoc />
    public void ScheduleExternalSync() => recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
        "external-sync",
        job => job.SyncExternalWeatherData(),
        "0 * * * *");
}
