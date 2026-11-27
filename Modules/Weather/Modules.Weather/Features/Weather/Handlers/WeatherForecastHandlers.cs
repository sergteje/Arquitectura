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
    : IRequestHandler<GetWeatherForecastQuery, PaginatedResponse<WeatherForecastDto>>
{
    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<WeatherForecastDto>, RequestError>> Handle(
        GetWeatherForecastQuery request,
        CancellationToken cancellationToken)
    {
        var result = await cache.GetOrSetAsync(
            request,
            token => repository.GetAllAsync(request.Pagination, token),
            cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="GetWeatherForecastByIdQuery"/> through the read-through cache.</summary>
/// <param name="repository">The forecast repository.</param>
/// <param name="cache">The distributed cache.</param>
public sealed class GetWeatherForecastByIdQueryHandler(
    IWeatherForecastRepository repository,
    IDistributedCacheService cache)
    : IRequestHandler<GetWeatherForecastByIdQuery, WeatherForecastDto>
{
    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto, RequestError>> Handle(
        GetWeatherForecastByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await cache.GetOrSetAsync(
            request,
            token => repository.GetByIdAsync(request.Id, token),
            cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="CreateWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class CreateWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<CreateWeatherForecastCommand, WeatherForecastDto>
{
    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto, RequestError>> Handle(
        CreateWeatherForecastCommand request,
        CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = 0;

        var result = await repository.CreateAsync(entity, cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="UpdateWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class UpdateWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<UpdateWeatherForecastCommand, WeatherForecastDto>
{
    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto, RequestError>> Handle(
        UpdateWeatherForecastCommand request,
        CancellationToken cancellationToken)
    {
        var entity = request.Request.Adapt<WeatherForecastDto>();
        entity.Id = request.Id;

        var result = await repository.UpdateAsync(request.Id, entity, cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="DeleteWeatherForecastCommand"/>.</summary>
/// <param name="repository">The forecast repository.</param>
public sealed class DeleteWeatherForecastCommandHandler(IWeatherForecastRepository repository)
    : IRequestHandler<DeleteWeatherForecastCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Result<Unit, RequestError>> Handle(
        DeleteWeatherForecastCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.DeleteAsync(request.Id, cancellationToken);

        return result.ToUnitRequestResult();
    }
}
