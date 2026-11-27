using BuildingBlocks.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Weather.Contracts;
using Modules.Weather.Infrastructure.Jobs;
using Modules.Weather.Infrastructure.Persistence;
using Modules.Weather.Persistence;

namespace Modules.Weather.Infrastructure.DependencyInjection;

/// <summary>
/// Composition root for the Weather module.
/// </summary>
public static class WeatherModuleExtensions
{
    /// <summary>
    /// Registers the Weather module: handlers, validators, persistence and recurring jobs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddWeatherModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var moduleAssembly = typeof(GetWeatherForecastQuery).Assembly;

        services.AddRequestHandlersFromAssembly(moduleAssembly);
        services.AddValidatorsFromAssembly(moduleAssembly);

        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

        return services;
    }

    /// <summary>
    /// Registers the Weather module's recurring jobs. Call this only when Hangfire is configured,
    /// because the scheduler depends on Hangfire's <c>IRecurringJobManager</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddWeatherModuleJobs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<WeatherForecastJobs>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }
}
