using ResulTus;

namespace Modules.DataTable.Errors;

/// <summary>Errors owned by the DataTable module.</summary>
[ResultErrors]
public static partial class DataTableErrors
{
    /// <summary>No table matches the key.</summary>
    /// <param name="tableKey">The table key that was searched.</param>
    /// <returns>A not-found error.</returns>
    [ErrorFactory("TABLE_NOT_FOUND", ErrorType.NotFound, "Table '{tableKey}' not found")]
    public static partial Error TableNotFound(string tableKey);

    /// <summary>Reading the table configuration failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("CONFIG_ERROR", ErrorType.Failure, "Configuration error: {detail}")]
    public static partial Error ConfigError(string detail);

    /// <summary>Saving the user configuration failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("SAVE_CONFIG_ERROR", ErrorType.Failure, "Saving configuration failed: {detail}")]
    public static partial Error SaveConfigError(string detail);

    /// <summary>Resetting the user configuration failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("RESET_CONFIG_ERROR", ErrorType.Failure, "Resetting configuration failed: {detail}")]
    public static partial Error ResetConfigError(string detail);

    /// <summary>Reading table data failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("DATA_ERROR", ErrorType.Failure, "Reading table data failed: {detail}")]
    public static partial Error DataError(string detail);

    /// <summary>Creating the export request failed.</summary>
    /// <param name="detail">The underlying failure detail.</param>
    /// <returns>A failure error.</returns>
    [ErrorFactory("EXPORT_ERROR", ErrorType.Failure, "Creating the export failed: {detail}")]
    public static partial Error ExportError(string detail);
}
