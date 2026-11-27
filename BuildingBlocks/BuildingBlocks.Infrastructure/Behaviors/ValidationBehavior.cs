using BuildingBlocks.Abstractions.Mediator;
using FluentValidation;
using ResulTus;
using ResulTus.Validation;
using ResulTus.Validation.FluentValidation;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> for the request through the
/// <c>ResulTus.Validation.FluentValidation</c> adapter and short-circuits the pipeline with a
/// <see cref="RequestError"/> carrying the structured <see cref="ValidationErrors"/> when any rule fails.
/// </summary>
/// <remarks>
/// A rejected request is a failed result, not an exception: the error travels back to the caller and
/// the host renders the <c>errors</c> member of the RFC 9457 problem details from it.
/// </remarks>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The success payload type.</typeparam>
/// <param name="validators">The validators registered for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<Result<TResponse, RequestError>> Handle(
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
            return Result<TResponse, RequestError>.Failure(new RequestError(
                Error.Validation("VALIDATION_ERROR", "One or more validation errors occurred."),
                new ValidationErrors(issues)));
        }

        return await next(cancellationToken);
    }
}
