using FluentValidation;
using Arquitectura.Domain.Models;

namespace Arquitectura.Application.Features.WeatherForecast.Validators;

/// <summary>
/// Validator for CreateWeatherForecastRequest
/// </summary>
public class CreateWeatherForecastValidator : AbstractValidator<CreateWeatherForecastRequest>
{
    public CreateWeatherForecastValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past");
        
        RuleFor(x => x.TemperatureC)
            .InclusiveBetween(-100, 100)
            .WithMessage("Temperature must be between -100 and 100°C");
        
        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required")
            .MaximumLength(500)
            .WithMessage("Summary cannot exceed 500 characters")
            .When(x => x.Summary != null);
    }
}

/// <summary>
/// Validator for UpdateWeatherForecastRequest
/// </summary>
public class UpdateWeatherForecastValidator : AbstractValidator<CreateWeatherForecastRequest>
{
    public UpdateWeatherForecastValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required");
        
        RuleFor(x => x.TemperatureC)
            .InclusiveBetween(-100, 100)
            .WithMessage("Temperature must be between -100 and 100°C");
        
        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required")
            .MaximumLength(500)
            .WithMessage("Summary cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for PaginationRequest
/// </summary>
public class PaginationRequestValidator : AbstractValidator<Domain.Models.Pagination.PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
