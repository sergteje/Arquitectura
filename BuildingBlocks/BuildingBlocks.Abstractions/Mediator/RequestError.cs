using ResulTus;
using ResulTus.Validation;

namespace BuildingBlocks.Abstractions.Mediator;

/// <summary>
/// The failure of a request pipeline: a domain <see cref="Error"/> plus the structured
/// <see cref="ValidationErrors"/> when the failure came from request validation.
/// </summary>
/// <param name="Error">The domain error that describes the failure.</param>
/// <param name="ValidationErrors">The field-level validation errors, or <c>null</c> for a domain failure.</param>
public sealed record RequestError(Error Error, ValidationErrors? ValidationErrors = null)
{
    /// <summary>Creates a request error from a domain error.</summary>
    /// <param name="error">The domain error.</param>
    /// <returns>The request error.</returns>
    public static RequestError From(Error error) => new(error);
}
