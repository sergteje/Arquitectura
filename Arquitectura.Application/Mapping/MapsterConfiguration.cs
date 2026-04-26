using Mapster;
using Arquitectura.Domain.Models;

namespace Arquitectura.Application.Mapping;

/// <summary>
/// Centralized Mapster configuration
/// </summary>
public static class MapsterConfiguration
{
    public static void Configure()
    {
        // WeatherForecast mappings - Dto is already an Entity, so direct mapping
        TypeAdapterConfig<WeatherForecastDto, WeatherForecastDto>
            .NewConfig();
        
        // CreateWeatherForecastRequest -> WeatherForecastDto (Entity)
        TypeAdapterConfig<CreateWeatherForecastRequest, WeatherForecastDto>
            .NewConfig()
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.TemperatureC, src => src.TemperatureC)
            .Map(dest => dest.Summary, src => src.Summary);
    }
}

/// <summary>
/// Extension for easy mapping using Mapster
/// </summary>
public static class Mapper
{
    public static void Initialize()
    {
        MapsterConfiguration.Configure();
    }
    
    public static TDestination Map<TDestination>(object source)
    {
        return source.Adapt<TDestination>();
    }
    
    public static TDestination Map<TSource, TDestination>(TSource source)
    {
        return source.Adapt<TSource, TDestination>();
    }
}
