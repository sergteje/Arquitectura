using BuildingBlocks.Abstractions.Mediator;

namespace Modules.Weather.Contracts;

/// <summary>Creates a forecast and evicts the module cache.</summary>
/// <param name="Request">The forecast payload.</param>
public record CreateWeatherForecastCommand(CreateWeatherForecastRequest Request)
    : ICommand<WeatherForecastDto>, ICacheInvalidatingRequest
{
    /// <inheritdoc />
    public IReadOnlyList<string> CacheKeyPrefixes => WeatherCacheKeys.Prefixes;
}

/// <summary>Updates a forecast and evicts the module cache.</summary>
/// <param name="Id">The forecast identifier.</param>
/// <param name="Request">The forecast payload.</param>
public record UpdateWeatherForecastCommand(int Id, CreateWeatherForecastRequest Request)
    : ICommand<WeatherForecastDto>, ICacheInvalidatingRequest
{
    /// <inheritdoc />
    public IReadOnlyList<string> CacheKeyPrefixes => WeatherCacheKeys.Prefixes;
}

/// <summary>Deletes a forecast and evicts the module cache.</summary>
/// <param name="Id">The forecast identifier.</param>
public record DeleteWeatherForecastCommand(int Id)
    : ICommand<Unit>, ICacheInvalidatingRequest
{
    /// <inheritdoc />
    public IReadOnlyList<string> CacheKeyPrefixes => WeatherCacheKeys.Prefixes;
}

/// <summary>Shared cache-key conventions for the Weather module.</summary>
public static class WeatherCacheKeys
{
    /// <summary>The prefix shared by every Weather cache entry.</summary>
    public const string Prefix = "weatherforecast";

    /// <summary>The prefixes evicted after a write.</summary>
    public static readonly IReadOnlyList<string> Prefixes = [Prefix];

    /// <summary>Builds the cache key for a paginated list.</summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The cache key.</returns>
    public static string ForPage(int page, int pageSize) => $"{Prefix}:all:{page}:{pageSize}";

    /// <summary>Builds the cache key for a single forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <returns>The cache key.</returns>
    public static string ForId(int id) => $"{Prefix}:{id}";
}
