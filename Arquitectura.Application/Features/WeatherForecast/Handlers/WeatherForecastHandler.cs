using MediatR;
using Mapster;
using Arquitectura.Domain.Models;
using Arquitectura.Domain.Models.Pagination;
using Arquitectura.Domain.Interfaces;
using Arquitectura.Application.Features.WeatherForecast.Queries;
using Arquitectura.Application.Features.WeatherForecast.Commands;

namespace Arquitectura.Application.Features.WeatherForecast.Handlers;

public class GetWeatherForecastHandler(IWeatherForecastRepository repository) : IRequestHandler<GetWeatherForecastQuery, Result<PaginatedResponse<WeatherForecastDto>>>
{
    public async Task<Result<PaginatedResponse<WeatherForecastDto>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(request.Pagination, cancellationToken);
    }
}

public class GetWeatherForecastByIdHandler(IWeatherForecastRepository repository) : IRequestHandler<GetWeatherForecastByIdQuery, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(GetWeatherForecastByIdQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(request.Id, cancellationToken);
    }
}

public class CreateWeatherForecastHandler(IWeatherForecastRepository repository) : IRequestHandler<CreateWeatherForecastCommand, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(CreateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = 0;
        return await repository.CreateAsync(entity, cancellationToken);
    }
}

public class UpdateWeatherForecastHandler(IWeatherForecastRepository repository) : IRequestHandler<UpdateWeatherForecastCommand, Result<WeatherForecastDto>>
{
    public async Task<Result<WeatherForecastDto>> Handle(UpdateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = request.Id;
        return await repository.UpdateAsync(request.Id, entity, cancellationToken);
    }
}

public class DeleteWeatherForecastHandler(IWeatherForecastRepository repository) : IRequestHandler<DeleteWeatherForecastCommand, Result>
{
    public async Task<Result> Handle(DeleteWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        return await repository.DeleteAsync(request.Id, cancellationToken);
    }
}
