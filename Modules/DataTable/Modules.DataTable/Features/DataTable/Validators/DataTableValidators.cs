using FluentValidation;
using Modules.DataTable.Contracts;

namespace Modules.DataTable.Features.DataTable.Validators;

/// <summary>Validates <see cref="GetDataTableConfigQuery"/>.</summary>
public sealed class GetDataTableConfigQueryValidator : AbstractValidator<GetDataTableConfigQuery>
{
    /// <summary>Initializes the validator.</summary>
    public GetDataTableConfigQueryValidator()
    {
        RuleFor(x => x.TableKey).NotEmpty().WithMessage("TableKey is required");
    }
}

/// <summary>Validates <see cref="SaveDataTableConfigCommand"/>.</summary>
public sealed class SaveDataTableConfigCommandValidator : AbstractValidator<SaveDataTableConfigCommand>
{
    /// <summary>Initializes the validator.</summary>
    public SaveDataTableConfigCommandValidator()
    {
        RuleFor(x => x.TableKey).NotEmpty().WithMessage("TableKey is required");
        RuleFor(x => x.Config).NotNull().WithMessage("Config is required");
        RuleFor(x => x.Config.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0");
    }
}

/// <summary>Validates <see cref="GetDataTableDataQuery"/>.</summary>
public sealed class GetDataTableDataQueryValidator : AbstractValidator<GetDataTableDataQuery>
{
    /// <summary>Initializes the validator.</summary>
    public GetDataTableDataQueryValidator()
    {
        RuleFor(x => x.TableKey).NotEmpty().WithMessage("TableKey is required");
        RuleFor(x => x.Request.Page).GreaterThan(0).WithMessage("Page must be greater than 0");
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
