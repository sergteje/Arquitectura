using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Arquitectura.Domain.Extensions;
using Arquitectura.Domain.Models;
using Arquitectura.Application.Features.DataTable.Queries;

namespace Arquitectura.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("DataTable")]
[Authorize]
[EnableRateLimiting("fixed")]
public class DataTableController(ISender mediator) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirst("userId")?.Value ?? "0");

    [HttpGet("{tableKey}")]
    [ProducesResponseType(typeof(DataTableConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfig(string tableKey, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetDataTableConfigQuery(tableKey, userId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{tableKey}/config")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveConfig(
        string tableKey,
        [FromBody] SaveDataTableConfigRequestDto config,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new SaveDataTableConfigQuery(tableKey, userId, config), cancellationToken);
        return result.IsSuccess ? NoContent() : ResultExtensions.MapError(result.Error!);
    }

    [HttpDelete("{tableKey}/config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetConfig(string tableKey, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new ResetDataTableConfigQuery(tableKey, userId), cancellationToken);
        return result.IsSuccess ? NoContent() : ResultExtensions.MapError(result.Error!);
    }

    [HttpGet("{tableKey}/data")]
    [ProducesResponseType(typeof(DataResponseDto<Dictionary<string, object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetData(
        string tableKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string direction = "asc",
        [FromQuery] string filters = "{}",
        CancellationToken cancellationToken = default)
    {
        var request = new DataTableRequestDto
        {
            Page = page,
            PageSize = pageSize,
            Sort = sort,
            Direction = direction,
            Filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(filters) ?? new()
        };

        var result = await mediator.Send(new GetDataTableDataQuery(tableKey, request), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{tableKey}/export")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(DataTableExportResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExport(
        string tableKey,
        [FromBody] DataTableExportRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var exportRequest = new DataTableExportRequestDto
        {
            TableKey = tableKey,
            Filters = request.Filters,
            SortColumn = request.SortColumn,
            SortDirection = request.SortDirection
        };
        var result = await mediator.Send(new CreateDataTableExportQuery(userId, exportRequest), cancellationToken);
        return result.IsSuccess 
            ? Accepted(result.Value!) 
            : ResultExtensions.MapError(result.Error!);
    }
}