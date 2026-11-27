using Arquitectura.Host.Web;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Abstractions.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Modules.Weather.Contracts;

namespace Arquitectura.Host.Controllers;

/// <summary>Weather forecast endpoints.</summary>
/// <param name="sender">The mediator.</param>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Weather Forecast")]
[Authorize]
[EnableRateLimiting("fixed")]
public sealed class WeatherForecastController(ISender sender) : ControllerBase
{
    /// <summary>Reads one page of forecasts.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the page, or a mapped error.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<WeatherForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await sender.Send(new GetWeatherForecastQuery(pagination), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Reads a single forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the forecast, or a mapped error.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWeatherForecastByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Creates a forecast.</summary>
    /// <param name="request">The forecast payload.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>201 with the created forecast, or a mapped error.</returns>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateWeatherForecastCommand(request), cancellationToken);
        return result.Match<IActionResult>(
            forecast => CreatedAtAction(nameof(GetById), new { id = forecast.Id }, forecast),
            ResultHttpExtensions.ToErrorResult);
    }

    /// <summary>Updates a forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="request">The new values.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the updated forecast, or a mapped error.</returns>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateWeatherForecastCommand(id, request), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Deletes a forecast.</summary>
    /// <param name="id">The forecast identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>204, or a mapped error.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWeatherForecastCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }
}
