using System.Data;
using System.Text.Json;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using Modules.DataTable.Contracts;
using Modules.DataTable.Persistence;
using ResulTus;

namespace Modules.DataTable.Infrastructure.Persistence;

/// <summary>
/// Dapper implementation of <see cref="IDataTableRepository"/> over the dynamic-table stored procedures.
/// </summary>
/// <param name="connectionFactory">The connection factory.</param>
public sealed class DataTableRepository(IDbConnectionFactory connectionFactory) : IDataTableRepository
{
    /// <inheritdoc />
    public async Task<Result<DataTableConfigResponseDto>> GetTableConfigAsync(
        string tableKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                "sp_GetTableConfig",
                new { TableKey = tableKey, UserId = userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var columns = (await multi.ReadAsync<dynamic>()).ToList();
            var preferences = await multi.ReadFirstOrDefaultAsync<dynamic>();
            var defaultPageSize = await multi.ReadFirstAsync<int>();

            if (columns.Count == 0)
            {
                return Result<DataTableConfigResponseDto>.Failure(
                    Error.NotFound("TABLE_NOT_FOUND", $"Table '{tableKey}' not found"));
            }

            var allColumns = columns.Select(column => new ColumnConfigDto(
                Key: (string)column.ColumnKey,
                Label: (string)column.DisplayName,
                DataType: (string)column.DataType,
                Sortable: (bool)column.IsSortable,
                Filterable: (bool)column.IsFilterable,
                IsVisibleByDefault: (bool)column.IsVisibleByDefault,
                IsFilterVisibleByDefault: (bool)column.IsFilterVisibleByDefault,
                IsAlwaysVisible: (bool)column.IsAlwaysVisible,
                SortOrder: (int)column.SortOrder,
                Width: (int?)column.Width,
                FilterType: (string?)column.FilterType,
                FilterOptionsSource: (string?)column.FilterOptionsSource)).ToList();

            var visibleKeys = new List<string>();
            var activeFilters = new List<string>();
            var currentFilters = new Dictionary<string, object>();
            var sort = new SortConfigDto();
            var pageSize = defaultPageSize;

            if (preferences is not null)
            {
                if (preferences.VisibleColumns is not null)
                {
                    visibleKeys = JsonSerializer.Deserialize<List<string>>((string)preferences.VisibleColumns)
                        ?? DefaultVisible(allColumns);
                }

                if (preferences.ActiveFilters is not null)
                {
                    activeFilters = JsonSerializer.Deserialize<List<string>>((string)preferences.ActiveFilters)
                        ?? DefaultFilters(allColumns);
                }

                if (preferences.FilterValues is not null)
                {
                    currentFilters = JsonSerializer.Deserialize<Dictionary<string, object>>((string)preferences.FilterValues)
                        ?? [];
                }

                if (preferences.SortConfig is not null)
                {
                    sort = JsonSerializer.Deserialize<SortConfigDto>((string)preferences.SortConfig)
                        ?? new SortConfigDto();
                }

                if (preferences.PageSize is not null)
                {
                    pageSize = (int)preferences.PageSize;
                }
            }
            else
            {
                visibleKeys = DefaultVisible(allColumns);
                activeFilters = DefaultFilters(allColumns);
            }

            foreach (var key in allColumns.Where(column => column.IsAlwaysVisible).Select(column => column.Key))
            {
                if (!visibleKeys.Contains(key))
                {
                    visibleKeys.Add(key);
                }
            }

            var response = new DataTableConfigResponseDto
            {
                TableId = 0,
                Columns = allColumns,
                VisibleColumnKeys = visibleKeys,
                AvailableFilters = allColumns.Where(column => column.Filterable).Select(column => column.Key).ToList(),
                ActiveFilterKeys = activeFilters,
                CurrentFilters = currentFilters,
                Sort = sort,
                PageSize = pageSize
            };

            return Result<DataTableConfigResponseDto>.Success(response);
        }
        catch (Exception exception)
        {
            return Result<DataTableConfigResponseDto>.Failure(Error.Failure("CONFIG_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveUserConfigAsync(
        string tableKey,
        int userId,
        SaveDataTableConfigRequestDto config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var parameters = new
            {
                UserId = userId,
                TableKey = tableKey,
                VisibleColumns = JsonSerializer.Serialize(config.VisibleColumns),
                ActiveFilters = JsonSerializer.Serialize(config.ActiveFilters),
                FilterValues = JsonSerializer.Serialize(config.FilterValues),
                SortConfig = JsonSerializer.Serialize(config.SortConfig),
                config.PageSize
            };

            await connection.ExecuteAsync(new CommandDefinition(
                "sp_SaveUserTableConfig",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(Error.Failure("SAVE_CONFIG_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result> ResetUserConfigAsync(
        string tableKey,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            await connection.ExecuteAsync(new CommandDefinition(
                "sp_DeleteUserTableConfig",
                new { UserId = userId, TableKey = tableKey },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(Error.Failure("RESET_CONFIG_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<DataResponseDto<Dictionary<string, object>>>> GetTableDataAsync(
        string tableKey,
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();

            var parameters = new
            {
                TableKey = tableKey,
                request.Page,
                request.PageSize,
                SortColumn = request.Sort,
                SortDirection = request.Direction,
                FiltersJson = JsonSerializer.Serialize(request.Filters)
            };

            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                "sp_GetTableData",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var data = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            var rows = data
                .Select(row => ((IDictionary<string, object>)row).ToDictionary(pair => pair.Key, pair => pair.Value))
                .ToList();

            var response = new DataResponseDto<Dictionary<string, object>>
            {
                Data = rows,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return Result<DataResponseDto<Dictionary<string, object>>>.Success(response);
        }
        catch (Exception exception)
        {
            return Result<DataResponseDto<Dictionary<string, object>>>.Failure(
                Error.Failure("DATA_ERROR", exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<DataTableExportResponseDto>> CreateExportRequestAsync(
        int userId,
        DataTableExportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestId = Guid.NewGuid();

            await using var connection = connectionFactory.CreateConnection();

            var parameters = new
            {
                RequestId = requestId,
                UserId = userId,
                request.TableKey,
                FiltersJson = JsonSerializer.Serialize(request.Filters),
                request.SortColumn,
                request.SortDirection,
                Status = "Pending"
            };

            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO ExportRequests (RequestId, UserId, TableKey, FiltersJson, SortColumn, SortDirection, Status, CreatedAt)
                VALUES (@RequestId, @UserId, @TableKey, @FiltersJson, @SortColumn, @SortDirection, @Status, GETUTCDATE())
                """,
                parameters,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken));

            return Result<DataTableExportResponseDto>.Success(new DataTableExportResponseDto
            {
                RequestId = requestId,
                Message = "Export started"
            });
        }
        catch (Exception exception)
        {
            return Result<DataTableExportResponseDto>.Failure(Error.Failure("EXPORT_ERROR", exception.Message));
        }
    }

    private static List<string> DefaultVisible(List<ColumnConfigDto> columns) =>
        columns.Where(column => column.IsVisibleByDefault).Select(column => column.Key).ToList();

    private static List<string> DefaultFilters(List<ColumnConfigDto> columns) =>
        columns.Where(column => column.IsFilterVisibleByDefault).Select(column => column.Key).ToList();
}
