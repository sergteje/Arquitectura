namespace Modules.DataTable.Contracts;

/// <summary>Metadata for one table column.</summary>
/// <param name="Key">The column key.</param>
/// <param name="Label">The display label.</param>
/// <param name="DataType">The column data type.</param>
/// <param name="Sortable">Whether the column can be sorted.</param>
/// <param name="Filterable">Whether the column can be filtered.</param>
/// <param name="IsVisibleByDefault">Whether the column starts visible.</param>
/// <param name="IsFilterVisibleByDefault">Whether the filter starts visible.</param>
/// <param name="IsAlwaysVisible">Whether the column cannot be hidden.</param>
/// <param name="SortOrder">The display order.</param>
/// <param name="Width">The optional column width.</param>
/// <param name="FilterType">The optional filter type.</param>
/// <param name="FilterOptionsSource">The optional filter options source.</param>
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
    string? FilterOptionsSource);

/// <summary>Sort configuration.</summary>
/// <param name="Column">The sorted column, or <c>null</c>.</param>
/// <param name="Direction">The sort direction.</param>
public record SortConfigDto(string? Column = null, string Direction = "asc");

/// <summary>Full table configuration returned to the client.</summary>
public class DataTableConfigResponseDto
{
    /// <summary>Gets or sets the table identifier.</summary>
    public int TableId { get; init; }

    /// <summary>Gets or sets every column.</summary>
    public List<ColumnConfigDto> Columns { get; init; } = [];

    /// <summary>Gets or sets the visible column keys.</summary>
    public List<string> VisibleColumnKeys { get; init; } = [];

    /// <summary>Gets or sets the available filter keys.</summary>
    public List<string> AvailableFilters { get; init; } = [];

    /// <summary>Gets or sets the active filter keys.</summary>
    public List<string> ActiveFilterKeys { get; init; } = [];

    /// <summary>Gets or sets the current filter values.</summary>
    public Dictionary<string, object> CurrentFilters { get; init; } = [];

    /// <summary>Gets or sets the sort configuration.</summary>
    public SortConfigDto Sort { get; init; } = new();

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; init; }
}

/// <summary>User preferences to persist.</summary>
public class SaveDataTableConfigRequestDto
{
    /// <summary>Gets or sets the visible column keys.</summary>
    public List<string> VisibleColumns { get; init; } = [];

    /// <summary>Gets or sets the active filter keys.</summary>
    public List<string> ActiveFilters { get; init; } = [];

    /// <summary>Gets or sets the filter values.</summary>
    public Dictionary<string, object> FilterValues { get; init; } = [];

    /// <summary>Gets or sets the sort configuration.</summary>
    public SortConfigDto SortConfig { get; init; } = new();

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; init; }
}

/// <summary>Request for one page of table data.</summary>
public class DataTableRequestDto
{
    /// <summary>Gets or sets the one-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; init; } = 10;

    /// <summary>Gets or sets the sort column.</summary>
    public string? Sort { get; init; }

    /// <summary>Gets or sets the sort direction.</summary>
    public string Direction { get; init; } = "asc";

    /// <summary>Gets or sets the filters.</summary>
    public Dictionary<string, object> Filters { get; init; } = [];
}

/// <summary>One page of table data.</summary>
/// <typeparam name="T">The row type.</typeparam>
public class DataResponseDto<T>
{
    /// <summary>Gets or sets the rows on this page.</summary>
    public List<T> Data { get; init; } = [];

    /// <summary>Gets or sets the total row count.</summary>
    public int Total { get; init; }

    /// <summary>Gets or sets the page number.</summary>
    public int Page { get; init; }

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; init; }
}

/// <summary>Export request.</summary>
public class DataTableExportRequestDto
{
    /// <summary>Gets or sets the table key.</summary>
    public string TableKey { get; init; } = string.Empty;

    /// <summary>Gets or sets the filters.</summary>
    public Dictionary<string, object> Filters { get; init; } = [];

    /// <summary>Gets or sets the sort column.</summary>
    public string? SortColumn { get; init; }

    /// <summary>Gets or sets the sort direction.</summary>
    public string SortDirection { get; init; } = "asc";
}

/// <summary>Export acknowledgement.</summary>
public class DataTableExportResponseDto
{
    /// <summary>Gets or sets the export request identifier.</summary>
    public Guid RequestId { get; init; }

    /// <summary>Gets or sets the human-readable message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional download URL.</summary>
    public string? DownloadUrl { get; init; }

    /// <summary>Gets or sets the optional file name.</summary>
    public string? FileName { get; init; }
}
