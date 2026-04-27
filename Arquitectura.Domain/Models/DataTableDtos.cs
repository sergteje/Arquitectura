namespace Arquitectura.Domain.Models;

/// <summary>
/// DTOs for DataTable dynamic configuration
/// </summary>

// 1. ColumnConfigDto: metadatos de una columna
public record ColumnConfigDto(
    string Key,
    string Label,
    string DataType,
    bool Sortable,
    bool Filterable,
    bool IsVisibleByDefault,
    bool IsFilterVisibleByDefault,
    bool IsAlwaysVisible,
    int SortOrder,
    int? Width,
    string? FilterType,
    string? FilterOptionsSource
);

// 2. SortConfigDto: configuración de ordenamiento
public record SortConfigDto(string? Column = null, string Direction = "asc");

// 3. DataTableConfigResponseDto: respuesta de configuración completa
public class DataTableConfigResponseDto
{
    public int TableId { get; init; }
    public List<ColumnConfigDto> Columns { get; init; } = new();
    public List<string> VisibleColumnKeys { get; init; } = new();
    public List<string> AvailableFilters { get; init; } = new();
    public List<string> ActiveFilterKeys { get; init; } = new();
    public Dictionary<string, object> CurrentFilters { get; init; } = new();
    public SortConfigDto Sort { get; init; } = new();
    public int PageSize { get; init; }
}

// 4. SaveDataTableConfigRequestDto: request para guardar preferencias
public class SaveDataTableConfigRequestDto
{
    public List<string> VisibleColumns { get; init; } = new();
    public List<string> ActiveFilters { get; init; } = new();
    public Dictionary<string, object> FilterValues { get; init; } = new();
    public SortConfigDto SortConfig { get; init; } = new();
    public int PageSize { get; init; }
}

// 5. DataTableRequestDto: request para obtener datos de tabla
public class DataTableRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Sort { get; init; }
    public string Direction { get; init; } = "asc";
    public Dictionary<string, object> Filters { get; init; } = new();
}

// 6. DataResponseDto: respuesta de datos de tabla
public class DataResponseDto<T>
{
    public List<T> Data { get; init; } = new();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

// 7. DataTableExportRequestDto: request para exportación
public class DataTableExportRequestDto
{
    public string TableKey { get; init; } = string.Empty;
    public Dictionary<string, object> Filters { get; init; } = new();
    public string? SortColumn { get; init; }
    public string SortDirection { get; init; } = "asc";
}

// 8. DataTableExportResponseDto: respuesta de exportación
public class DataTableExportResponseDto
{
    public Guid RequestId { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? DownloadUrl { get; init; }
    public string? FileName { get; init; }
}