using BuildingBlocks.Abstractions.Pagination;
using Modules.Weather.Contracts;
using ResulTus;

namespace Modules.Weather.Persistence;

/// <summary>
/// Persistence port for weather forecasts. Implemented by the module infrastructure over stored procedures.
/// </summary>
public interface IWeatherForecastRepository
{
    /// <summary>Reads one page of forecasts.</summary>
    /// <param name="pagination">The page selector.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The requested page.</returns>
    Task<Result<PaginatedResponse<WeatherForecastDto>>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    /// <summary>Reads a single forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The forecast, or a not-found error.</returns>
    Task<Result<WeatherForecastDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a forecast.</summary>
    /// <param name="entity">The forecast to create.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The created forecast.</returns>
    Task<Result<WeatherForecastDto>> CreateAsync(
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="entity">The new values.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The updated forecast.</returns>
    Task<Result<WeatherForecastDto>> UpdateAsync(
        int id,
        WeatherForecastDto entity,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>A success or failure result.</returns>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
