using FluentValidation;
using Modules.Auth.Contracts;

namespace Modules.Auth.Features.Auth.Validators;

/// <summary>Validates <see cref="LoginCommand"/>.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes the validator.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid address");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

/// <summary>Validates <see cref="RegisterCommand"/>.</summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>Initializes the validator.</summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid address");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");
    }
}

/// <summary>Validates <see cref="RefreshTokenCommand"/>.</summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>Initializes the validator.</summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
