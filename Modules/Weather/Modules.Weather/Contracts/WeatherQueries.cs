using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Abstractions.Pagination;

namespace Modules.Weather.Contracts;

/// <summary>Reads one page of forecasts; cached and retried because it is idempotent.</summary>
/// <param name="Pagination">The page selector.</param>
public record GetWeatherForecastQuery(PaginationRequest Pagination)
    : IQuery<PaginatedResponse<WeatherForecastDto>>, ICachableQuery, IResilientRequest
{
    /// <inheritdoc />
    public string GetCacheKey() => WeatherCacheKeys.ForPage(Pagination.Page, Pagination.PageSize);
}

/// <summary>Reads a single forecast; cached and retried because it is idempotent.</summary>
/// <param name="Id">The forecast identifier.</param>
public record GetWeatherForecastByIdQuery(int Id)
    : IQuery<WeatherForecastDto>, ICachableQuery, IResilientRequest
{
    /// <inheritdoc />
    public string GetCacheKey() => WeatherCacheKeys.ForId(Id);
}
