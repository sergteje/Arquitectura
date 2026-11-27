using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Abstractions.Pagination;
using BuildingBlocks.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Modules.Weather.Contracts;

namespace Modules.Weather.Api;

/// <summary>Minimal API endpoints owned by the Weather module.</summary>
public static class WeatherEndpoints
{
    /// <summary>Maps the Weather module endpoints.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/weatherforecast")
            .WithTags("Weather Forecast")
            .RequireAuthorization()
            .RequireRateLimiting("fixed");

        group.MapGet("/", async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var pagination = new PaginationRequest
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 20
                };

                var result = await sender.Send(new GetWeatherForecastQuery(pagination), cancellationToken);
                return result.ToHttpResult();
            })
            .Produces<PaginatedResponse<WeatherForecastDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
                (await sender.Send(new GetWeatherForecastByIdQuery(id), cancellationToken)).ToHttpResult())
            .Produces<WeatherForecastDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/", async (
                CreateWeatherForecastRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateWeatherForecastCommand(request), cancellationToken);

                return result.Match<IResult>(
                    forecast => Results.Created($"/api/weatherforecast/{forecast.Id}", forecast),
                    error => error.ToProblemHttpResult());
            })
            .Produces<WeatherForecastDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPut("/{id:int}", async (
                int id,
                CreateWeatherForecastRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
                (await sender.Send(new UpdateWeatherForecastCommand(id, request), cancellationToken)).ToHttpResult())
            .Produces<WeatherForecastDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
                (await sender.Send(new DeleteWeatherForecastCommand(id), cancellationToken)).ToNoContentHttpResult())
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
