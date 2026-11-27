using ResulTus.Validation;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Thrown by <see cref="ValidationBehavior{TRequest,TResponse}"/> when one or more FluentValidation
/// rules fail. Carries the structured <see cref="ValidationErrors"/> so the host can render the
/// RFC 9457 <c>errors</c> member.
/// </summary>
/// <param name="errors">The structured validation errors.</param>
public sealed class RequestValidationException(ValidationErrors errors)
    : Exception("One or more validation errors occurred.")
{
    /// <summary>Gets the structured validation errors.</summary>
    public ValidationErrors Errors { get; } = errors;
}
