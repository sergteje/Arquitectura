using System.Data;
using System.Data.Common;
using System.Text.Json;
using BuildingBlocks.Abstractions.Persistence;
using Dapper;
using Modules.DataTable.Contracts;
using Modules.DataTable.Persistence;
using ResulTus;
using ResulTus.Extensions;

using Modules.DataTable.Errors;

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
        var config = await ExecuteAsync(
            async (connection, token) =>
            {
                using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
                    "sp_GetTableConfig",
                    new { TableKey = tableKey, UserId = userId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: token));

                var columns = (await multi.ReadAsync<dynamic>()).ToList();
                var preferences = await multi.ReadFirstOrDefaultAsync<dynamic>();
                var defaultPageSize = await multi.ReadFirstAsync<int>();

                if (columns.Count == 0)
                {
                    return null;
                }

                return BuildConfig(columns, preferences, defaultPageSize);
            },
            DataTableErrors.ConfigError,
            cancellationToken);

        return config.Match(
            value => value is null
                ? Result<DataTableConfigResponseDto>.Failure(DataTableErrors.TableNotFound(tableKey))
                : Result<DataTableConfigResponseDto>.Success(value),
            error => Result<DataTableConfigResponseDto>.Failure(error));
    }

    /// <inheritdoc />
    public Task<Result> SaveUserConfigAsync(
        string tableKey,
        int userId,
        SaveDataTableConfigRequestDto config,
        CancellationToken cancellationToken = default)
    {
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

        return ExecuteUnitAsync(
            (connection, token) => connection.ExecuteAsync(new CommandDefinition(
                "sp_SaveUserTableConfig",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: token)),
            DataTableErrors.SaveConfigError,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result> ResetUserConfigAsync(
        string tableKey,
        int userId,
        CancellationToken cancellationToken = default) =>
        ExecuteUnitAsync(
            (connection, token) => connection.ExecuteAsync(new CommandDefinition(
                "sp_DeleteUserTableConfig",
                new { UserId = userId, TableKey = tableKey },
                commandType: CommandType.StoredProcedure,
                cancellationToken: token)),
            DataTableErrors.ResetConfigError,
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<DataResponseDto<Dictionary<string, object>>>> GetTableDataAsync(
        string tableKey,
        DataTableRequestDto request,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            async (connection, token) =>
            {
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
                    cancellationToken: token));

                var data = (await multi.ReadAsync<dynamic>()).ToList();
                var total = await multi.ReadFirstAsync<int>();

                var rows = data
                    .Select(row => ((IDictionary<string, object>)row)
                        .ToDictionary(pair => pair.Key, pair => pair.Value))
                    .ToList();

                return new DataResponseDto<Dictionary<string, object>>
                {
                    Data = rows,
                    Total = total,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            },
            DataTableErrors.DataError,
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<DataTableExportResponseDto>> CreateExportRequestAsync(
        int userId,
        DataTableExportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid();

        return ExecuteAsync(
            async (connection, token) =>
            {
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
                    cancellationToken: token));

                return new DataTableExportResponseDto
                {
                    RequestId = requestId,
                    Message = "Export started"
                };
            },
            DataTableErrors.ExportError,
            cancellationToken);
    }

    private static DataTableConfigResponseDto BuildConfig(
        List<dynamic> columns,
        dynamic? preferences,
        int defaultPageSize)
    {
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

        return new DataTableConfigResponseDto
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
    }

    private static List<string> DefaultVisible(List<ColumnConfigDto> columns) =>
        columns.Where(column => column.IsVisibleByDefault).Select(column => column.Key).ToList();

    private static List<string> DefaultFilters(List<ColumnConfigDto> columns) =>
        columns.Where(column => column.IsFilterVisibleByDefault).Select(column => column.Key).ToList();

    private async Task<Result<T>> ExecuteAsync<T>(
        Func<DbConnection, CancellationToken, Task<T>> operation,
        Func<string, Error> errorFactory,
        CancellationToken cancellationToken)
    {
        var result = await Result.TryAsync(
            async token =>
            {
                await using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync(token);
                return await operation(connection, token);
            },
            cancellationToken);

        return result.MapError(error => errorFactory(error.Message ?? "Unknown database error"));
    }

    private async Task<Result> ExecuteUnitAsync(
        Func<DbConnection, CancellationToken, Task> operation,
        Func<string, Error> errorFactory,
        CancellationToken cancellationToken)
    {
        var result = await Result.TryAsync(
            async token =>
            {
                await using var connection = connectionFactory.CreateConnection();
                await connection.OpenAsync(token);
                await operation(connection, token);
            },
            cancellationToken);

        return result.MapError(error => errorFactory(error.Message ?? "Unknown database error"));
    }
}
