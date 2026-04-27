using MediatR;
using Arquitectura.Domain.Models;
using Arquitectura.Persistence.Repositories;
using Arquitectura.Application.Features.DataTable.Queries;

namespace Arquitectura.Application.Features.DataTable.Handlers;

public class GetDataTableConfigHandler(IDataTableRepository repository) : IRequestHandler<GetDataTableConfigQuery, Result<DataTableConfigResponseDto>>
{
    public async Task<Result<DataTableConfigResponseDto>> Handle(GetDataTableConfigQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetTableConfigAsync(request.TableKey, request.UserId, cancellationToken);
    }
}

public class SaveDataTableConfigHandler(IDataTableRepository repository) : IRequestHandler<SaveDataTableConfigQuery, Result>
{
    public async Task<Result> Handle(SaveDataTableConfigQuery request, CancellationToken cancellationToken)
    {
        return await repository.SaveUserConfigAsync(request.TableKey, request.UserId, request.Config, cancellationToken);
    }
}

public class ResetDataTableConfigHandler(IDataTableRepository repository) : IRequestHandler<ResetDataTableConfigQuery, Result>
{
    public async Task<Result> Handle(ResetDataTableConfigQuery request, CancellationToken cancellationToken)
    {
        return await repository.ResetUserConfigAsync(request.TableKey, request.UserId, cancellationToken);
    }
}

public class GetDataTableDataHandler(IDataTableRepository repository) : IRequestHandler<GetDataTableDataQuery, Result<DataResponseDto<Dictionary<string, object>>>>
{
    public async Task<Result<DataResponseDto<Dictionary<string, object>>>> Handle(GetDataTableDataQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetTableDataAsync(request.TableKey, request.Request, cancellationToken);
    }
}

public class CreateDataTableExportHandler(IDataTableRepository repository) : IRequestHandler<CreateDataTableExportQuery, Result<DataTableExportResponseDto>>
{
    public async Task<Result<DataTableExportResponseDto>> Handle(CreateDataTableExportQuery request, CancellationToken cancellationToken)
    {
        return await repository.CreateExportRequestAsync(request.UserId, request.Request, cancellationToken);
    }
}