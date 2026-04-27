using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Arquitectura.Domain.Extensions;
using Arquitectura.Domain.Models;
using Arquitectura.Domain.Models.Pagination;
using Arquitectura.Application.Features.WeatherForecast.Commands;
using Arquitectura.Application.Features.WeatherForecast.Queries;
using MediatR;

namespace Arquitectura.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Weather Forecast")]
[Authorize]
[EnableRateLimiting("fixed")]
public class WeatherForecastController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<WeatherForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await mediator.Send(new GetWeatherForecastQuery(pagination), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWeatherForecastByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateWeatherForecastCommand(request), cancellationToken);
        return result.Match(
            onSuccess: forecast => CreatedAtAction(nameof(GetById), new { id = forecast.Id }, forecast),
            onFailure: ResultExtensions.MapError);
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateWeatherForecastCommand(id, request), cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteWeatherForecastCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ResultExtensions.MapError(result.Error!);
    }
}