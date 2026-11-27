using Modules.DataTable.Contracts;
using ResulTus;

namespace Modules.DataTable.Persistence;

/// <summary>
/// Persistence port for dynamic table configuration and data.
/// </summary>
public interface IDataTableRepository
{
    /// <summary>Reads the table configuration for a user.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The table configuration.</returns>
    Task<Result<DataTableConfigResponseDto>> GetTableConfigAsync(
        string tableKey,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists the user's table preferences.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="config">The preferences to persist.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>A success or failure result.</returns>
    Task<Result> SaveUserConfigAsync(
        string tableKey,
        int userId,
        SaveDataTableConfigRequestDto config,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes the user's table preferences.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>A success or failure result.</returns>
    Task<Result> ResetUserConfigAsync(
        string tableKey,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one page of table data.</summary>
    /// <param name="tableKey">The table key.</param>
    /// <param name="request">The paging, sort and filter request.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The requested page.</returns>
    Task<Result<DataResponseDto<Dictionary<string, object>>>> GetTableDataAsync(
        string tableKey,
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an export request.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The export acknowledgement.</returns>
    Task<Result<DataTableExportResponseDto>> CreateExportRequestAsync(
        int userId,
        DataTableExportRequestDto request,
        CancellationToken cancellationToken = default);
}
