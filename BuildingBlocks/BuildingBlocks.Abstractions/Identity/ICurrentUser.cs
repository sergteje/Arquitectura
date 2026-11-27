using ResulTus;

namespace BuildingBlocks.Abstractions.Identity;

/// <summary>
/// The authenticated caller of the current operation, resolved from the transport (HTTP) by the host.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the user identifier, or <c>null</c> when the request is anonymous.
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Gets the user e-mail, or <c>null</c> when the request is anonymous.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current request carries an authenticated identity.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the authenticated user identifier as a result, or an unauthorized failure when the
    /// request is anonymous. This is the railway-oriented view used by endpoint compositions.
    /// </summary>
    Result<int> UserIdResult => UserId is { } id
        ? Result<int>.Success(id)
        : Result<int>.Failure(Error.Unauthorized("AUTH_REQUIRED", "Authentication is required."));
}
