namespace Arquitectura.Host.Web;

/// <summary>
/// The error body returned to clients.
/// </summary>
/// <remarks>
/// Serialized with the default camel-case policy this produces
/// <c>{ "code": string, "message": string, "details": object|null }</c>, which is the shape the
/// existing frontend already consumes.
/// </remarks>
/// <param name="Code">A stable, machine-readable error code.</param>
/// <param name="Message">A human-readable message.</param>
/// <param name="Details">Optional structured details, or <c>null</c>.</param>
public sealed record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, object>? Details = null);
