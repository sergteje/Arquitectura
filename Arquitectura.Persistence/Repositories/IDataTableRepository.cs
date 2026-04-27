using Arquitectura.Domain.Models;
using Arquitectura.Persistence;

namespace Arquitectura.Persistence.Repositories;

/// <summary>
/// Repository for DataTable configuration (metadata + user preferences)
/// </summary>
public interface IDataTableRepository
{
    Task<Result<DataTableConfigResponseDto>> GetTableConfigAsync(string tableKey, int userId, CancellationToken cancellationToken = default);
    Task<Result> SaveUserConfigAsync(string tableKey, int userId, SaveDataTableConfigRequestDto config, CancellationToken cancellationToken = default);
    Task<Result> ResetUserConfigAsync(string tableKey, int userId, CancellationToken cancellationToken = default);
    Task<Result<DataResponseDto<Dictionary<string, object>>>> GetTableDataAsync(string tableKey, DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<DataTableExportResponseDto>> CreateExportRequestAsync(int userId, DataTableExportRequestDto request, CancellationToken cancellationToken = default);
}