using System.Text.Json;
using BuildingBlocks.Abstractions.Identity;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.AspNetCore;
using ResulTus.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Modules.DataTable.Contracts;

namespace Modules.DataTable.Api;

/// <summary>Minimal API endpoints owned by the DataTable module.</summary>
public static class DataTableEndpoints
{
    /// <summary>Maps the DataTable module endpoints.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapDataTableEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/datatable")
            .WithTags("DataTable")
            .RequireAuthorization()
            .RequireRateLimiting("fixed");

        group.MapGet("/{tableKey}", async (
                string tableKey,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUser.RequireUserId();
                var result = await sender.Send(new GetDataTableConfigQuery(tableKey, userId), cancellationToken);
                return result.ToHttpResult();
            })
            .Produces<DataTableConfigResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{tableKey}/config", async (
                string tableKey,
                SaveDataTableConfigRequestDto config,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUser.RequireUserId();
                var result = await sender.Send(
                    new SaveDataTableConfigCommand(tableKey, userId, config),
                    cancellationToken);

                return result.ToNoContentHttpResult();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{tableKey}/config", async (
                string tableKey,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUser.RequireUserId();
                var result = await sender.Send(
                    new ResetDataTableConfigCommand(tableKey, userId),
                    cancellationToken);

                return result.ToNoContentHttpResult();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{tableKey}/data", async (
                string tableKey,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sort,
                [FromQuery] string? direction,
                [FromQuery] string? filters,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var parsedFilters = JsonSerializer.Deserialize<Dictionary<string, object>>(filters ?? "{}") ?? [];

                var request = new DataTableRequestDto
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10,
                    Sort = sort,
                    Direction = direction ?? "asc",
                    Filters = parsedFilters
                };

                var result = await sender.Send(new GetDataTableDataQuery(tableKey, request), cancellationToken);
                return result.ToHttpResult();
            })
            .Produces<DataResponseDto<Dictionary<string, object>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{tableKey}/export", async (
                string tableKey,
                DataTableExportRequestDto request,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                var userId = currentUser.RequireUserId();
                var exportRequest = new DataTableExportRequestDto
                {
                    TableKey = tableKey,
                    Filters = request.Filters,
                    SortColumn = request.SortColumn,
                    SortDirection = request.SortDirection
                };

                var result = await sender.Send(
                    new CreateDataTableExportCommand(userId, exportRequest),
                    cancellationToken);

                return result.Match<IResult>(
                    value => Results.Accepted(value: value),
                    error => error.ToProblemHttpResult());
            })
            .Produces<DataTableExportResponseDto>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
