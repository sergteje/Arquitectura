using MediatR;
using Arquitectura.Application.Interfaces;
using Arquitectura.Domain.Models;
using Arquitectura.Domain.Models.Pagination;

namespace Arquitectura.Application.Features.WeatherForecast.Queries;

public record GetWeatherForecastQuery(PaginationRequest Pagination) 
    : IRequest<Result<PaginatedResponse<WeatherForecastDto>>>, ICachableQuery
{
    public string GetCacheKey() => $"weatherforecasts_page{Pagination.Page}_size{Pagination.PageSize}";
}

public record GetWeatherForecastByIdQuery(int Id)  : IRequest<Result<WeatherForecastDto>>, ICachableQuery
{
    public string GetCacheKey() => $"weatherforecast_{Id}";
}