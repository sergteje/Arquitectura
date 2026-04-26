using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Arquitectura.Persistence.Repositories;
using Arquitectura.Persistence.UnitOfWork;
using Arquitectura.Persistence;
using Arquitectura.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arquitectura.Persistence.DependencyInjection;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure centralized database
        DatabaseConfiguration.Configure(configuration);
        
        // Register IDbConnectionFactory - injectable factory using centralized config
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        // Register IDbConnection for compatibility (health checks, etc) - uses factory
        services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<IDbConnectionFactory>().CreateConnection());
        
        // Register Unit of Work factory
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        
        // Register repositories
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, 
        string connectionString)
    {
        // Configure centralized database with custom string
        DatabaseConfiguration.Configure(connectionString);
        
        // Register IDbConnectionFactory - injectable factory using centralized config
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        // Register IDbConnection for compatibility (health checks, etc) - uses factory
        services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<IDbConnectionFactory>().CreateConnection());
        
        // Register Unit of Work factory
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        
        // Register repositories
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}