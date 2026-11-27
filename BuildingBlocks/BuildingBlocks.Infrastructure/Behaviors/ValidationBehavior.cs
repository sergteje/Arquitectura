using BuildingBlocks.Abstractions.Mediator;
using FluentValidation;
using ResulTus.Validation;
using ResulTus.Validation.FluentValidation;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> for the request through the
/// <c>ResulTus.Validation.FluentValidation</c> adapter and throws a
/// <see cref="RequestValidationException"/> carrying structured <see cref="ValidationErrors"/> when
/// any rule fails. The host renders those errors as RFC 9457 problem details.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="validators">The validators registered for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var issues = new List<ValidationIssue>();

        foreach (var validator in validators)
        {
            var (isSuccess, _, errors) = await request.ValidateAsync(validator, cancellationToken);

            if (!isSuccess && errors is not null)
            {
                issues.AddRange(errors.Issues);
            }
        }

        if (issues.Count > 0)
        {
            throw new RequestValidationException(new ValidationErrors(issues));
        }

        return await next(cancellationToken);
    }
}
