using Arquitectura.Domain.Models;
using Arquitectura.Domain.Models.Pagination;

namespace Arquitectura.Domain.Interfaces;

public interface IWeatherForecastRepository
{
    Task<Result<PaginatedResponse<WeatherForecastDto>>> GetAllAsync(PaginationRequest pagination, CancellationToken ct = default);
    Task<Result<WeatherForecastDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<WeatherForecastDto>> CreateAsync(WeatherForecastDto entity, CancellationToken ct = default);
    Task<Result<WeatherForecastDto>> UpdateAsync(int id, WeatherForecastDto entity, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}