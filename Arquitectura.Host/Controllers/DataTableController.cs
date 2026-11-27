using System.Text.Json;
using Arquitectura.Host.Web;
using BuildingBlocks.Abstractions.Identity;
using BuildingBlocks.Abstractions.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Modules.DataTable.Contracts;

namespace Arquitectura.Host.Controllers;

/// <summary>Dynamic data-table endpoints.</summary>
/// <param name="sender">The mediator.</param>
/// <param name="currentUser">The current user resolver.</param>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("DataTable")]
[Authorize]
[EnableRateLimiting("fixed")]
public sealed class DataTableController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Reads the table configuration for the current user.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the configuration, or a mapped error.</returns>
    [HttpGet("{tableKey}")]
    [ProducesResponseType(typeof(DataTableConfigResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfig(string tableKey, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var result = await sender.Send(new GetDataTableConfigQuery(tableKey, userId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Persists the current user's table preferences.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="config">The preferences to persist.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>204, or a mapped error.</returns>
    [HttpPost("{tableKey}/config")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveConfig(
        string tableKey,
        [FromBody] SaveDataTableConfigRequestDto config,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var result = await sender.Send(new SaveDataTableConfigCommand(tableKey, userId, config), cancellationToken);
        return result.ToNoContentResult();
    }

    /// <summary>Deletes the current user's table preferences.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>204, or a mapped error.</returns>
    [HttpDelete("{tableKey}/config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetConfig(string tableKey, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var result = await sender.Send(new ResetDataTableConfigCommand(tableKey, userId), cancellationToken);
        return result.ToNoContentResult();
    }

    /// <summary>Reads one page of table data.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="sort">The sort column.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="filters">A JSON object of filters.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the page, or a mapped error.</returns>
    [HttpGet("{tableKey}/data")]
    [ProducesResponseType(typeof(DataResponseDto<Dictionary<string, object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetData(
        string tableKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string direction = "asc",
        [FromQuery] string filters = "{}",
        CancellationToken cancellationToken = default)
    {
        var parsedFilters = JsonSerializer.Deserialize<Dictionary<string, object>>(filters) ?? [];

        var request = new DataTableRequestDto
        {
            Page = page,
            PageSize = pageSize,
            Sort = sort,
            Direction = direction,
            Filters = parsedFilters
        };

        var result = await sender.Send(new GetDataTableDataQuery(tableKey, request), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Creates an export request for the current user.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>202 with the acknowledgement, or a mapped error.</returns>
    [HttpPost("{tableKey}/export")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(DataTableExportResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExport(
        string tableKey,
        [FromBody] DataTableExportRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var exportRequest = new DataTableExportRequestDto
        {
            TableKey = tableKey,
            Filters = request.Filters,
            SortColumn = request.SortColumn,
            SortDirection = request.SortDirection
        };

        var result = await sender.Send(new CreateDataTableExportCommand(userId, exportRequest), cancellationToken);
        return result.Match<IActionResult>(
            value => Accepted(value),
            ResultHttpExtensions.ToErrorResult);
    }
}
