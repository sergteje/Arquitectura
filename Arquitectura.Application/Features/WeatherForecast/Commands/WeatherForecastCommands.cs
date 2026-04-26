using MediatR;
using Arquitectura.Domain.Models;

namespace Arquitectura.Application.Features.WeatherForecast.Commands;

public record CreateWeatherForecastCommand(CreateWeatherForecastRequest Request) 
    : IRequest<Result<WeatherForecastDto>>;

public record UpdateWeatherForecastCommand(int Id, CreateWeatherForecastRequest Request) 
    : IRequest<Result<WeatherForecastDto>>;

public record DeleteWeatherForecastCommand(int Id) 
    : IRequest<Result>;