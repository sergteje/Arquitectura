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
    /// Gets the authenticated user identifier or throws when the caller is anonymous.
    /// </summary>
    /// <returns>The authenticated user identifier.</returns>
    /// <exception cref="UnauthorizedAccessException">The current request is anonymous.</exception>
    int RequireUserId();
}
