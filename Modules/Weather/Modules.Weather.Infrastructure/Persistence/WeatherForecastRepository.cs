using System.Data;
using System.Data.Common;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Pagination;
using Dapper;
using Modules.Weather.Contracts;
using Modules.Weather.Persistence;
using ResulTus;
using ResulTus.Extensions;

using BuildingBlocks.Abstractions.Errors;
using Modules.Weather.Errors;

namespace Modules.Weather.Infrastructure.Persistence;

/// <summary>
/// Dapper implementation of <see cref="IWeatherForecastRepository"/> over the
/// <c>dbo.WeatherForecast_*</c> stored procedures.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class WeatherForecastRepository(IDbConnectionFactory connectionFactory) : IWeatherForecastRepository
{
    /// <inheritdoc />
    public Task<Result<PaginatedResponse<WeatherForecastDto>>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            async (connection, token) =>
            {
                var parameters = new { pagination.Page, pagination.PageSize };

                using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                    "dbo.WeatherForecast_GetAll",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token));

                var rawItems = (await multi.ReadAsync<dynamic>()).ToList();
                var totalCount = await multi.ReadSingleAsync<int>();

                var items = rawItems.Select(ToDto).ToList();

                return PaginatedResponse<WeatherForecastDto>.Create(
                    items,
                    pagination.Page,
                    pagination.PageSize,
                    totalCount);
            },
            cancellationToken);

    /// <inheritdoc />
    public async Task<Result<WeatherForecastDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var loaded = await ExecuteAsync(
            async (connection, token) =>
            {
                var raw = await connection.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
                    "dbo.WeatherForecast_GetById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token));

                return raw is null ? null : ToDto(raw);
            },
            cancellationToken);

        return loaded.Match(
            item => item is null
                ? Result<WeatherForecastDto>.Failure(WeatherErrors.ForecastNotFound(id))
                : Result<WeatherForecastDto>.Success(item),
            error => Result<WeatherForecastDto>.Failure(error));
    }

    /// <inheritdoc />
    public Task<Result<WeatherForecastDto>> CreateAsync(
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            async (connection, token) =>
            {
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
                    cancellationToken: token));

                return new WeatherForecastDto
                {
                    Id = id,
                    Date = entity.Date,
                    TemperatureC = entity.TemperatureC,
                    Summary = entity.Summary
                };
            },
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<WeatherForecastDto>> UpdateAsync(
        int id,
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            async (connection, token) =>
            {
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
                    cancellationToken: token));

                return new WeatherForecastDto
                {
                    Id = id,
                    Date = entity.Date,
                    TemperatureC = entity.TemperatureC,
                    Summary = entity.Summary
                };
            },
            cancellationToken);

    /// <inheritdoc />
    public Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        ExecuteUnitAsync(
            (connection, token) => connection.ExecuteAsync(new CommandDefinition(
                "dbo.WeatherForecast_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: token)),
            cancellationToken);

    private static WeatherForecastDto ToDto(dynamic raw) => new()
    {
        Id = (int)raw.Id,
        Date = DateOnly.FromDateTime((DateTime)raw.Date),
        TemperatureC = (int)raw.TemperatureC,
        Summary = (string?)raw.Summary ?? string.Empty
    };

    private async Task<Result<T>> ExecuteAsync<T>(
        Func<DbConnection, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var result = await Result.TryAsync(
            async token =>
            {
                await using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync(token);
                return await operation(connection, token);
            },
            cancellationToken);

        return result.MapError(error => CommonErrors.Database(error.Message ?? "Unknown database error"));
    }

    private async Task<Result> ExecuteUnitAsync(
        Func<DbConnection, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var result = await Result.TryAsync(
            async token =>
            {
                await using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync(token);
                await operation(connection, token);
            },
            cancellationToken);

        return result.MapError(error => CommonErrors.Database(error.Message ?? "Unknown database error"));
    }
}
