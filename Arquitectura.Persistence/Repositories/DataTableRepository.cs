using Dapper;
using System.Data;
using Arquitectura.Domain.Models;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.Repositories;

/// <summary>
/// DataTable Repository - acceso a metadatos de tablas dinámicas
/// </summary>
public class DataTableRepository(IDbConnectionFactory connectionFactory) : IDataTableRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<DataTableConfigResponseDto>> GetTableConfigAsync(string tableKey, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var parameters = new DynamicParameters();
            parameters.Add("@TableKey", tableKey);
            parameters.Add("@UserId", userId);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetTableConfig",
                parameters,
                commandType: CommandType.StoredProcedure);

            // 1. Columnas
            var columns = (await multi.ReadAsync<dynamic>()).ToList();
            
            // 2. Preferencias del usuario
            var prefs = await multi.ReadFirstOrDefaultAsync<dynamic>();
            
            // 3. DefaultPageSize
            var defaultPageSize = await multi.ReadFirstAsync<int>();

            if (columns.Count == 0)
                return Result<DataTableConfigResponseDto>.Failure("TABLE_NOT_FOUND", $"Table '{tableKey}' not found");

            // Mapear columnas
            var allColumns = columns.Select(c => new ColumnConfigDto(
                Key: c.ColumnKey,
                Label: c.DisplayName,
                DataType: c.DataType,
                Sortable: c.IsSortable,
                Filterable: c.IsFilterable,
                IsVisibleByDefault: c.IsVisibleByDefault,
                IsFilterVisibleByDefault: c.IsFilterVisibleByDefault,
                IsAlwaysVisible: c.IsAlwaysVisible,
                SortOrder: c.SortOrder,
                Width: c.Width,
                FilterType: c.FilterType,
                FilterOptionsSource: c.FilterOptionsSource
            )).ToList();

            // Determinar visibles
            var visibleKeys = new List<string>();
            var activeFilters = new List<string>();
            var currentFilters = new Dictionary<string, object>();
            var sort = new SortConfigDto();
            var pageSize = defaultPageSize;

            if (prefs != null)
            {
                if (prefs.VisibleColumns != null)
                    visibleKeys = System.Text.Json.JsonSerializer.Deserialize<List<string>>(prefs.VisibleColumns) ?? allColumns.Where(c => c.IsVisibleByDefault).Select(c => c.Key).ToList();
                if (prefs.ActiveFilters != null)
                    activeFilters = System.Text.Json.JsonSerializer.Deserialize<List<string>>(prefs.ActiveFilters) ?? allColumns.Where(c => c.IsFilterVisibleByDefault).Select(c => c.Key).ToList();
                if (prefs.FilterValues != null)
                    currentFilters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(prefs.FilterValues) ?? new Dictionary<string, object>();
                if (prefs.SortConfig != null)
                    sort = System.Text.Json.JsonSerializer.Deserialize<SortConfigDto>(prefs.SortConfig) ?? new SortConfigDto();
                if (prefs.PageSize != null)
                    pageSize = (int)prefs.PageSize;
            }
            else
            {
                // Defaults
                visibleKeys = allColumns.Where(c => c.IsVisibleByDefault).Select(c => c.Key).ToList();
                activeFilters = allColumns.Where(c => c.IsFilterVisibleByDefault).Select(c => c.Key).ToList();
            }

            // Forzar columnas siempre visibles
            var alwaysVisible = allColumns.Where(c => c.IsAlwaysVisible).Select(c => c.Key);
            foreach (var key in alwaysVisible)
                if (!visibleKeys.Contains(key))
                    visibleKeys.Add(key);

            var availableFilters = allColumns.Where(c => c.Filterable).Select(c => c.Key).ToList();

            var response = new DataTableConfigResponseDto
            {
                TableId = 0,
                Columns = allColumns,
                VisibleColumnKeys = visibleKeys,
                AvailableFilters = availableFilters,
                ActiveFilterKeys = activeFilters,
                CurrentFilters = currentFilters,
                Sort = sort,
                PageSize = pageSize
            };

            return Result<DataTableConfigResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<DataTableConfigResponseDto>.Failure("CONFIG_ERROR", ex.Message);
        }
    }

    public async Task<Result> SaveUserConfigAsync(string tableKey, int userId, SaveDataTableConfigRequestDto config, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@TableKey", tableKey);
            parameters.Add("@VisibleColumns", System.Text.Json.JsonSerializer.Serialize(config.VisibleColumns));
            parameters.Add("@ActiveFilters", System.Text.Json.JsonSerializer.Serialize(config.ActiveFilters));
            parameters.Add("@FilterValues", System.Text.Json.JsonSerializer.Serialize(config.FilterValues));
            parameters.Add("@SortConfig", System.Text.Json.JsonSerializer.Serialize(config.SortConfig));
            parameters.Add("@PageSize", config.PageSize);

            await connection.ExecuteAsync(
                "sp_SaveUserTableConfig",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("SAVE_CONFIG_ERROR", ex.Message);
        }
    }

    public async Task<Result> ResetUserConfigAsync(string tableKey, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@TableKey", tableKey);

            await connection.ExecuteAsync(
                "sp_DeleteUserTableConfig",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("RESET_CONFIG_ERROR", ex.Message);
        }
    }

    public async Task<Result<DataResponseDto<Dictionary<string, object>>>> GetTableDataAsync(string tableKey, DataTableRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TableKey", tableKey);
            parameters.Add("@Page", request.Page);
            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@SortColumn", request.Sort);
            parameters.Add("@SortDirection", request.Direction);
            parameters.Add("@FiltersJson", System.Text.Json.JsonSerializer.Serialize(request.Filters));

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetTableData",
                parameters,
                commandType: CommandType.StoredProcedure);

            var data = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            var rows = data.Select(row => ((IDictionary<string, object>)row).ToDictionary(k => k.Key, v => v.Value)).ToList();

            var response = new DataResponseDto<Dictionary<string, object>>
            {
                Data = rows,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return Result<DataResponseDto<Dictionary<string, object>>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<DataResponseDto<Dictionary<string, object>>>.Failure("DATA_ERROR", ex.Message);
        }
    }

    public async Task<Result<DataTableExportResponseDto>> CreateExportRequestAsync(int userId, DataTableExportRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestId = Guid.NewGuid();

            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@RequestId", requestId);
            parameters.Add("@UserId", userId);
            parameters.Add("@TableKey", request.TableKey);
            parameters.Add("@FiltersJson", System.Text.Json.JsonSerializer.Serialize(request.Filters));
            parameters.Add("@SortColumn", request.SortColumn);
            parameters.Add("@SortDirection", request.SortDirection);
            parameters.Add("@Status", "Pending");

            await connection.ExecuteAsync(
                @"INSERT INTO ExportRequests (RequestId, UserId, TableKey, FiltersJson, SortColumn, SortDirection, Status, CreatedAt)
                  VALUES (@RequestId, @UserId, @TableKey, @FiltersJson, @SortColumn, @SortDirection, @Status, GETUTCDATE())",
                parameters,
                commandType: CommandType.Text);

            return Result<DataTableExportResponseDto>.Success(new DataTableExportResponseDto
            {
                RequestId = requestId,
                Message = "Exportación iniciada"
            });
        }
        catch (Exception ex)
        {
            return Result<DataTableExportResponseDto>.Failure("EXPORT_ERROR", ex.Message);
        }
    }
}