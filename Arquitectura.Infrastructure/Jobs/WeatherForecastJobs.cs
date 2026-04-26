using Hangfire;

namespace Arquitectura.Infrastructure.Jobs;

public class WeatherForecastJobs(ILogger<WeatherForecastJobs> logger)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
    public async Task CleanupOldForecasts()
    {
        logger.LogInformation("Iniciando limpeza de forecasts antigos...");
        await Task.Delay(1000);
        logger.LogInformation("Limpeza de forecasts antigos concluída");
    }

    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task GenerateDailyReport()
    {
        logger.LogInformation("Gerando relatório diário...");
        await Task.Delay(2000);
        logger.LogInformation("Relatório diário gerado com sucesso");
    }

    public async Task CheckForExtremeWeather()
    {
        logger.LogInformation("Verificando condições climáticas extremas...");
        await Task.Delay(500);
        logger.LogInformation("Verificação de clima extremo concluída");
    }

    public async Task SyncExternalWeatherData()
    {
        logger.LogInformation("Sincronizando dados climáticos externos...");
        await Task.Delay(1500);
        logger.LogInformation("Sincronização de dados externos concluída");
    }
}

public interface IJobScheduler
{
    void ScheduleCleanupOldForecasts();
    void ScheduleDailyReport();
    void ScheduleWeatherCheck();
    void ScheduleExternalSync();
}

public class HangfireJobScheduler : IJobScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public void ScheduleCleanupOldForecasts()
    {
        _recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
            "cleanup-old-forecasts",
            job => job.CleanupOldForecasts(),
            "0 2 * * *");
    }

    public void ScheduleDailyReport()
    {
        _recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
            "daily-report",
            job => job.GenerateDailyReport(),
            "0 8 * * *");
    }

    public void ScheduleWeatherCheck()
    {
        _recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
            "weather-check",
            job => job.CheckForExtremeWeather(),
            "*/30 * * * *");
    }

    public void ScheduleExternalSync()
    {
        _recurringJobManager.AddOrUpdate<WeatherForecastJobs>(
            "external-sync",
            job => job.SyncExternalWeatherData(),
            "0 * * * *");
    }
}