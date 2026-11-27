using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Abstractions.Pagination;
using Mapster;
using Modules.Weather.Contracts;
using Modules.Weather.Persistence;
using ResulTus;

namespace Modules.Weather.Features.Weather.Handlers;

/// <summary>Handles <see cref="GetWeatherForecastQuery"/> through the read-through cache.</summary>
/// <param name="repository">The forecast repository.</param>
/// <param name="cache">The distributed cache.</param>
public sealed class GetWeatherForecastQueryHandler(
    IWeatherForecastRepository repository,
    IDistributedCacheService cache)
    : IRequestHandler<GetWeatherForecastQuery, Result<PaginatedResponse<WeatherForecastDto>>>
{
    /// <inheritdoc />
    public Task<Result<PaginatedResponse<WeatherForecastDto>>> Handle(
        GetWeatherForecastQuery request,
        CancellationToken cancellationToken)
        => cache.GetOrSetAsync(
            request,
            token => repository.GetAllAsync(request.Pagination, token),
            cancellationToken);
}

/// <summary>Handles <see cref="GetWeatherForecastByIdQuery"/> through the read-through cache.</summary>
/// <param name="repository">The forecast repository.</param>
/// <param name="cache">The distributed cache.</param>
public sealed class GetWeatherForecastByIdQueryHandler(
    IWeatherForecastRepository repository,
    IDistributedCacheService cache)
    : IRequestHandler<GetWeatherForecastByIdQuery, Result<WeatherForecastDto>>
{
    /// <inheritdoc />
    public Task<Result<WeatherForecastDto>> Handle(
        GetWeatherForecastByIdQuery request,
        CancellationToken cancellationToken)
        => cache.GetOrSetAsync(
            request,
            token => repository.GetByIdAsync(request.Id, token),
            cancellationToken);
}

/// <summary>Handles <see cref="CreateWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class CreateWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<CreateWeatherForecastCommand, Result<WeatherForecastDto>>
{
    /// <inheritdoc />
    public Task<Result<WeatherForecastDto>> Handle(
        CreateWeatherForecastCommand request,
        CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = 0;

        return repository.CreateAsync(entity, cancellationToken);
    }
}

/// <summary>Handles <see cref="UpdateWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class UpdateWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<UpdateWeatherForecastCommand, Result<WeatherForecastDto>>
{
    /// <inheritdoc />
    public Task<Result<WeatherForecastDto>> Handle(
        UpdateWeatherForecastCommand request,
        CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = request.Id;

        return repository.UpdateAsync(request.Id, entity, cancellationToken);
    }
}

/// <summary>Handles <see cref="DeleteWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class DeleteWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<DeleteWeatherForecastCommand, Result>
{
    /// <inheritdoc />
    public Task<Result> Handle(DeleteWeatherForecastCommand request, CancellationToken cancellationToken)
        => repository.DeleteAsync(request.Id, cancellationToken);
}
