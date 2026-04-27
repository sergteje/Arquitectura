using MediatR;
using Arquitectura.Domain.Models;

namespace Arquitectura.Application.Features.DataTable.Queries;

public record GetDataTableConfigQuery(string TableKey, int UserId) : IRequest<Result<DataTableConfigResponseDto>>;

public record SaveDataTableConfigQuery(string TableKey, int UserId, SaveDataTableConfigRequestDto Config) : IRequest<Result>;

public record ResetDataTableConfigQuery(string TableKey, int UserId) : IRequest<Result>;

public record GetDataTableDataQuery(string TableKey, DataTableRequestDto Request) : IRequest<Result<DataResponseDto<Dictionary<string, object>>>>;

public record CreateDataTableExportQuery(int UserId, DataTableExportRequestDto Request) : IRequest<Result<DataTableExportResponseDto>>;