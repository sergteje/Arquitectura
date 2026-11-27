using BuildingBlocks.Abstractions.Mediator;
using FluentValidation;
using Microsoft.Extensions.Logging;
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
/// <param name="logger">The logger that records which fields were rejected.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
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
            // Field names only: a rejected value is user input and must never reach the log.
            logger.LogInformation(
                "Validation rejected {RequestName} with {ValidationErrorCount} field errors: {ValidationFields}",
                typeof(TRequest).Name,
                issues.Count,
                string.Join(", ", issues.Select(issue => issue.Field).Distinct()));

            return Result<TResponse, RequestError>.Failure(new RequestError(
                Error.Validation("VALIDATION_ERROR", "One or more validation errors occurred."),
                new ValidationErrors(issues)));
        }

        return await next(cancellationToken);
    }
}
