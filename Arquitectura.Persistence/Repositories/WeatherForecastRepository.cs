using Dapper;
using Microsoft.Data.SqlClient;
using Arquitectura.Domain.Models;
using Arquitectura.Domain.Models.Pagination;
using Arquitectura.Domain.Interfaces;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.Repositories;

/// <summary>
/// WeatherForecast Repository using Dapper + Stored Procedures
/// Uses IDbConnectionFactory injected via constructor - follows DI pattern
/// </summary>
public class WeatherForecastRepository(IDbConnectionFactory connectionFactory) : IWeatherForecastRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<PaginatedResponse<WeatherForecastDto>>> GetAllAsync(PaginationRequest pagination, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(ct);
            
            var parameters = new DynamicParameters();
            parameters.Add("Page", pagination.Page);
            parameters.Add("PageSize", pagination.PageSize);
            
            using var multi = await connection.QueryMultipleAsync("dbo.WeatherForecast_GetAll", parameters, 
                commandType: System.Data.CommandType.StoredProcedure);
            
            // Read as dynamic to handle DateTime -> DateOnly conversion
            var rawItems = (await multi.ReadAsync<dynamic>()).ToList();
            var totalCount = await multi.ReadSingleAsync<int>();
            
            var items = rawItems.Select(raw => new WeatherForecastDto
            {
                Id = raw.Id,
                Date = DateOnly.FromDateTime((DateTime)raw.Date),
                TemperatureC = raw.TemperatureC,
                Summary = raw.Summary ?? string.Empty
            }).ToList();

            var response = PaginatedResponse<WeatherForecastDto>.Create(items, pagination.Page, pagination.PageSize, totalCount);
            return Result<PaginatedResponse<WeatherForecastDto>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResponse<WeatherForecastDto>>.Failure("DATABASE_ERROR", $"Error: {ex.Message}");
        }
    }

    public async Task<Result<WeatherForecastDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(ct);
            
            // Get raw data first to handle DateOnly conversion
            var raw = await connection.QuerySingleOrDefaultAsync<dynamic>(
                "dbo.WeatherForecast_GetById", new { Id = id }, commandType: System.Data.CommandType.StoredProcedure);
            
            if (raw == null)
                return Result<WeatherForecastDto>.Failure(ServiceErrors.NotFound("WeatherForecast", id.ToString()));
            
            var item = new WeatherForecastDto
            {
                Id = raw.Id,
                Date = DateOnly.FromDateTime((DateTime)raw.Date),
                TemperatureC = raw.TemperatureC,
                Summary = raw.Summary ?? string.Empty
            };
            
            return Result<WeatherForecastDto>.Success(item);
        }
        catch (Exception ex)
        {
            return Result<WeatherForecastDto>.Failure("DATABASE_ERROR", $"Error: {ex.Message}");
        }
    }

    public async Task<Result<WeatherForecastDto>> CreateAsync(WeatherForecastDto entity, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(ct);
            
            var dateValue = entity.Date.ToDateTime(TimeOnly.MinValue);
            var parameters = new { Date = dateValue, TemperatureC = entity.TemperatureC, Summary = entity.Summary };
            var id = await connection.ExecuteScalarAsync<int>("dbo.WeatherForecast_Create", parameters,
                commandType: System.Data.CommandType.StoredProcedure);
            
            var newEntity = new WeatherForecastDto
            {
                Id = id,
                Date = entity.Date,
                TemperatureC = entity.TemperatureC,
                Summary = entity.Summary
            };
            return Result<WeatherForecastDto>.Success(newEntity);
        }
        catch (Exception ex)
        {
            return Result<WeatherForecastDto>.Failure("DATABASE_ERROR", $"Error: {ex.Message}");
        }
    }

    public async Task<Result<WeatherForecastDto>> UpdateAsync(int id, WeatherForecastDto entity, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(ct);
            
            var dateValue = entity.Date.ToDateTime(TimeOnly.MinValue);
            var parameters = new { Id = id, Date = dateValue, TemperatureC = entity.TemperatureC, Summary = entity.Summary };
            await connection.ExecuteAsync("dbo.WeatherForecast_Update", parameters, commandType: System.Data.CommandType.StoredProcedure);
            
            var updatedEntity = new WeatherForecastDto
            {
                Id = id,
                Date = entity.Date,
                TemperatureC = entity.TemperatureC,
                Summary = entity.Summary
            };
            return Result<WeatherForecastDto>.Success(updatedEntity);
        }
        catch (Exception ex)
        {
            return Result<WeatherForecastDto>.Failure("DATABASE_ERROR", $"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(ct);
            
            await connection.ExecuteAsync("dbo.WeatherForecast_Delete", new { Id = id }, commandType: System.Data.CommandType.StoredProcedure);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("DATABASE_ERROR", $"Error: {ex.Message}");
        }
    }
}