using System.Data;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Pagination;
using Dapper;
using Modules.Weather.Contracts;
using Modules.Weather.Persistence;
using ResulTus;

namespace Modules.Weather.Infrastructure.Persistence;

/// <summary>
/// Dapper implementation of <see cref="IWeatherForecastRepository"/> over the
/// <c>dbo.WeatherForecast_*</c> stored procedures.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class WeatherForecastRepository(IDbConnectionFactory connectionFactory) : IWeatherForecastRepository
{
    /// <inheritdoc />
    public async Task<Result<PaginatedResponse<WeatherForecastDto>>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var parameters = new { pagination.Page, pagination.PageSize };

            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                "dbo.WeatherForecast_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var rawItems = (await multi.ReadAsync<dynamic>()).ToList();
            var totalCount = await multi.ReadSingleAsync<int>();

            var items = rawItems.Select(raw => new WeatherForecastDto
            {
                Id = (int)raw.Id,
                Date = DateOnly.FromDateTime((DateTime)raw.Date),
                TemperatureC = (int)raw.TemperatureC,
                Summary = (string?)raw.Summary ?? string.Empty
            }).ToList();

            var response = PaginatedResponse<WeatherForecastDto>.Create(
                items,
                pagination.Page,
                pagination.PageSize,
                totalCount);

            return Result<PaginatedResponse<WeatherForecastDto>>.Success(response);
        }
        catch (Exception exception)
        {
            return Result<PaginatedResponse<WeatherForecastDto>>.Failure(
                Error.Failure("DATABASE_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var raw = await connection.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
                "dbo.WeatherForecast_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            if (raw is null)
            {
                return Result<WeatherForecastDto>.Failure(
                    Error.NotFound("WEATHERFORECAST_NOT_FOUND", $"Weather forecast with id '{id}' was not found"));
            }

            var item = new WeatherForecastDto
            {
                Id = (int)raw.Id,
                Date = DateOnly.FromDateTime((DateTime)raw.Date),
                TemperatureC = (int)raw.TemperatureC,
                Summary = (string?)raw.Summary ?? string.Empty
            };

            return Result<WeatherForecastDto>.Success(item);
        }
        catch (Exception exception)
        {
            return Result<WeatherForecastDto>.Failure(Error.Failure("DATABASE_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto>> CreateAsync(
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var parameters = new
            {
                Date = entity.Date.ToDateTime(TimeOnly.MinValue),
                entity.TemperatureC,
                entity.Summary
            };

            var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "dbo.WeatherForecast_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var created = new WeatherForecastDto
            {
                Id = id,
                Date = entity.Date,
                TemperatureC = entity.TemperatureC,
                Summary = entity.Summary
            };

            return Result<WeatherForecastDto>.Success(created);
        }
        catch (Exception exception)
        {
            return Result<WeatherForecastDto>.Failure(Error.Failure("DATABASE_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto>> UpdateAsync(
        int id,
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var parameters = new
            {
                Id = id,
                Date = entity.Date.ToDateTime(TimeOnly.MinValue),
                entity.TemperatureC,
                entity.Summary
            };

            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.WeatherForecast_Update",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var updated = new WeatherForecastDto
            {
                Id = id,
                Date = entity.Date,
                TemperatureC = entity.TemperatureC,
                Summary = entity.Summary
            };

            return Result<WeatherForecastDto>.Success(updated);
        }
        catch (Exception exception)
        {
            return Result<WeatherForecastDto>.Failure(Error.Failure("DATABASE_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.WeatherForecast_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(Error.Failure("DATABASE_ERROR", exception.Message));
        }
    }
}
