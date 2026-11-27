using BuildingBlocks.Abstractions.Mediator;
using ResulTus;

namespace Modules.DataTable.Contracts;

/// <summary>Reads the table configuration for a user.</summary>
/// <param name="TableKey">The table key.</param>
/// <param name="UserId">The authenticated user identifier.</param>
public record GetDataTableConfigQuery(string TableKey, int UserId)
    : IQuery<Result<DataTableConfigResponseDto>>;

/// <summary>Persists the user's table preferences.</summary>
/// <param name="TableKey">The table key.</param>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Config">The preferences to persist.</param>
public record SaveDataTableConfigCommand(string TableKey, int UserId, SaveDataTableConfigRequestDto Config)
    : ICommand<Result>;

/// <summary>Deletes the user's table preferences.</summary>
/// <param name="TableKey">The table key.</param>
/// <param name="UserId">The authenticated user identifier.</param>
public record ResetDataTableConfigCommand(string TableKey, int UserId)
    : ICommand<Result>;

/// <summary>Reads one page of table data.</summary>
/// <param name="TableKey">The table key.</param>
/// <param name="Request">The paging, sort and filter request.</param>
public record GetDataTableDataQuery(string TableKey, DataTableRequestDto Request)
    : IQuery<Result<DataResponseDto<Dictionary<string, object>>>>;

/// <summary>Creates an export request.</summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Request">The export request.</param>
public record CreateDataTableExportCommand(int UserId, DataTableExportRequestDto Request)
    : ICommand<Result<DataTableExportResponseDto>>;
