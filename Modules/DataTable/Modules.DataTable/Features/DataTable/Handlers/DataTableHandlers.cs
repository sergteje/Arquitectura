using BuildingBlocks.Abstractions.Mediator;
using Modules.DataTable.Contracts;
using Modules.DataTable.Persistence;
using ResulTus;

namespace Modules.DataTable.Features.DataTable.Handlers;

/// <summary>Handles <see cref="GetDataTableConfigQuery"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class GetDataTableConfigQueryHandler(IDataTableRepository repository)
    : IRequestHandler<GetDataTableConfigQuery, Result<DataTableConfigResponseDto>>
{
    /// <inheritdoc />
    public Task<Result<DataTableConfigResponseDto>> Handle(
        GetDataTableConfigQuery request,
        CancellationToken cancellationToken)
        => repository.GetTableConfigAsync(request.TableKey, request.UserId, cancellationToken);
}

/// <summary>Handles <see cref="SaveDataTableConfigCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class SaveDataTableConfigCommandHandler(IDataTableRepository repository)
    : IRequestHandler<SaveDataTableConfigCommand, Result>
{
    /// <inheritdoc />
    public Task<Result> Handle(SaveDataTableConfigCommand request, CancellationToken cancellationToken)
        => repository.SaveUserConfigAsync(request.TableKey, request.UserId, request.Config, cancellationToken);
}

/// <summary>Handles <see cref="ResetDataTableConfigCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class ResetDataTableConfigCommandHandler(IDataTableRepository repository)
    : IRequestHandler<ResetDataTableConfigCommand, Result>
{
    /// <inheritdoc />
    public Task<Result> Handle(ResetDataTableConfigCommand request, CancellationToken cancellationToken)
        => repository.ResetUserConfigAsync(request.TableKey, request.UserId, cancellationToken);
}

/// <summary>Handles <see cref="GetDataTableDataQuery"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class GetDataTableDataQueryHandler(IDataTableRepository repository)
    : IRequestHandler<GetDataTableDataQuery, Result<DataResponseDto<Dictionary<string, object>>>>
{
    /// <inheritdoc />
    public Task<Result<DataResponseDto<Dictionary<string, object>>>> Handle(
        GetDataTableDataQuery request,
        CancellationToken cancellationToken)
        => repository.GetTableDataAsync(request.TableKey, request.Request, cancellationToken);
}

/// <summary>Handles <see cref="CreateDataTableExportCommand"/>.</summary>
/// <param name="repository">The data-table repository.</param>
public sealed class CreateDataTableExportCommandHandler(IDataTableRepository repository)
    : IRequestHandler<CreateDataTableExportCommand, Result<DataTableExportResponseDto>>
{
    /// <inheritdoc />
    public Task<Result<DataTableExportResponseDto>> Handle(
        CreateDataTableExportCommand request,
        CancellationToken cancellationToken)
        => repository.CreateExportRequestAsync(request.UserId, request.Request, cancellationToken);
}
