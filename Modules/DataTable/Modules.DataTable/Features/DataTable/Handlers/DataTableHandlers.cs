using BuildingBlocks.Abstractions.Mediator;
using Modules.DataTable.Contracts;
using Modules.DataTable.Persistence;
using ResulTus;

namespace Modules.DataTable.Features.DataTable.Handlers;

/// <summary>Handles <see cref="GetDataTableConfigQuery"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class GetDataTableConfigQueryHandler(IDataTableRepository repository)
    : IRequestHandler<GetDataTableConfigQuery, DataTableConfigResponseDto>
{
    /// <inheritdoc />
    public async Task<Result<DataTableConfigResponseDto, RequestError>> Handle(
        GetDataTableConfigQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetTableConfigAsync(request.TableKey, request.UserId, cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="SaveDataTableConfigCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class SaveDataTableConfigCommandHandler(IDataTableRepository repository)
    : IRequestHandler<SaveDataTableConfigCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Result<Unit, RequestError>> Handle(
        SaveDataTableConfigCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.SaveUserConfigAsync(
            request.TableKey,
            request.UserId,
            request.Config,
            cancellationToken);

        return result.ToUnitRequestResult();
    }
}

/// <summary>Handles <see cref="ResetDataTableConfigCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class ResetDataTableConfigCommandHandler(IDataTableRepository repository)
    : IRequestHandler<ResetDataTableConfigCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Result<Unit, RequestError>> Handle(
        ResetDataTableConfigCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.ResetUserConfigAsync(
            request.TableKey,
            request.UserId,
            cancellationToken);

        return result.ToUnitRequestResult();
    }
}

/// <summary>Handles <see cref="GetDataTableDataQuery"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class GetDataTableDataQueryHandler(IDataTableRepository repository)
    : IRequestHandler<GetDataTableDataQuery, DataResponseDto<Dictionary<string, object>>>
{
    /// <inheritdoc />
    public async Task<Result<DataResponseDto<Dictionary<string, object>>, RequestError>> Handle(
        GetDataTableDataQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetTableDataAsync(request.TableKey, request.Request, cancellationToken);

        return result.ToRequestResult();
    }
}

/// <summary>Handles <see cref="CreateDataTableExportCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class CreateDataTableExportCommandHandler(IDataTableRepository repository)
    : IRequestHandler<CreateDataTableExportCommand, DataTableExportResponseDto>
{
    /// <inheritdoc />
    public async Task<Result<DataTableExportResponseDto, RequestError>> Handle(
        CreateDataTableExportCommand request,
        CancellationToken cancellationToken)
    {
        var result = await repository.CreateExportRequestAsync(
            request.UserId,
            request.Request,
            cancellationToken);

        return result.ToRequestResult();
    }
}
