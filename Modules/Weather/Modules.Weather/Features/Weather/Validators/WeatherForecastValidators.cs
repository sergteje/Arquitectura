using FluentValidation;
using Modules.Weather.Contracts;

namespace Modules.Weather.Features.Weather.Validators;

/// <summary>Validates <see cref="CreateWeatherForecastCommand"/>.</summary>
public sealed class CreateWeatherForecastCommandValidator : AbstractValidator<CreateWeatherForecastCommand>
{
    /// <summary>Initializes the validator.</summary>
    public CreateWeatherForecastCommandValidator()
    {
        RuleFor(x => x.Request.Date)
            .NotEmpty().WithMessage("Date is required")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past");

        RuleFor(x => x.Request.TemperatureC)
            .InclusiveBetween(-100, 100).WithMessage("Temperature must be between -100 and 100 C");

        RuleFor(x => x.Request.Summary)
            .NotEmpty().WithMessage("Summary is required")
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters");
    }
}

/// <summary>Validates <see cref="UpdateWeatherForecastCommand"/>.</summary>
public sealed class UpdateWeatherForecastCommandValidator : AbstractValidator<UpdateWeatherForecastCommand>
{
    /// <summary>Initializes the validator.</summary>
    public UpdateWeatherForecastCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.Request.Date)
            .NotEmpty().WithMessage("Date is required");

        RuleFor(x => x.Request.TemperatureC)
            .InclusiveBetween(-100, 100).WithMessage("Temperature must be between -100 and 100 C");

        RuleFor(x => x.Request.Summary)
            .NotEmpty().WithMessage("Summary is required")
            .MaximumLength(500).WithMessage("Summary cannot exceed 500 characters");
    }
}

/// <summary>Validates <see cref="GetWeatherForecastQuery"/>.</summary>
public sealed class GetWeatherForecastQueryValidator : AbstractValidator<GetWeatherForecastQuery>
{
    /// <summary>Initializes the validator.</summary>
    public GetWeatherForecastQueryValidator()
    {
        RuleFor(x => x.Pagination.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.Pagination.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
    }
}
