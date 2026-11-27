using ResulTus;

namespace Modules.Auth.Errors;

/// <summary>Errors owned by the Auth module.</summary>
[ResultErrors]
public static partial class AuthErrors
{
    /// <summary>The credentials do not match a user.</summary>
    /// <returns>An unauthorized error.</returns>
    [ErrorFactory("AUTH_INVALID_CREDENTIALS", ErrorType.Unauthorized, "Invalid email or password")]
    public static partial Error InvalidCredentials();

    /// <summary>The account is disabled.</summary>
    /// <returns>A forbidden error.</returns>
    [ErrorFactory("AUTH_ACCOUNT_DISABLED", ErrorType.Forbidden, "Account is disabled")]
    public static partial Error AccountDisabled();

    /// <summary>Checking whether the user exists failed.</summary>
    /// <returns>A failure error.</returns>
    [ErrorFactory("AUTH_LOOKUP_FAILED", ErrorType.Failure, "Error checking user existence")]
    public static partial Error LookupFailed();

    /// <summary>The e-mail is already registered.</summary>
    /// <returns>A conflict error.</returns>
    [ErrorFactory("AUTH_EMAIL_EXISTS", ErrorType.Conflict, "Email already registered")]
    public static partial Error EmailExists();

    /// <summary>Creating the user failed.</summary>
    /// <returns>A failure error.</returns>
    [ErrorFactory("AUTH_CREATE_FAILED", ErrorType.Failure, "Failed to create user")]
    public static partial Error CreateFailed();

    /// <summary>Updating the user failed.</summary>
    /// <returns>A failure error.</returns>
    [ErrorFactory("AUTH_UPDATE_FAILED", ErrorType.Failure, "Failed to update user")]
    public static partial Error UpdateFailed();

    /// <summary>The refresh token is not valid.</summary>
    /// <returns>An unauthorized error.</returns>
    [ErrorFactory("AUTH_INVALID_REFRESH_TOKEN", ErrorType.Unauthorized, "Invalid refresh token")]
    public static partial Error InvalidRefreshToken();

    /// <summary>The user owning the refresh token does not exist.</summary>
    /// <returns>An unauthorized error.</returns>
    [ErrorFactory("AUTH_USER_NOT_FOUND", ErrorType.Unauthorized, "User not found")]
    public static partial Error UserNotFound();

    /// <summary>The refresh token expired or was rotated.</summary>
    /// <returns>An unauthorized error.</returns>
    [ErrorFactory("AUTH_REFRESH_TOKEN_EXPIRED", ErrorType.Unauthorized, "Refresh token expired")]
    public static partial Error RefreshTokenExpired();

    /// <summary>Logging out failed.</summary>
    /// <returns>A failure error.</returns>
    [ErrorFactory("AUTH_LOGOUT_FAILED", ErrorType.Failure, "Failed to logout")]
    public static partial Error LogoutFailed();

    /// <summary>No user matches the e-mail.</summary>
    /// <param name="email">The e-mail that was searched.</param>
    /// <returns>A not-found error.</returns>
    [ErrorFactory("USER_NOT_FOUND", ErrorType.NotFound, "User with email '{email}' was not found")]
    public static partial Error UserNotFoundByEmail(string email);

    /// <summary>No user matches the identifier.</summary>
    /// <param name="id">The identifier that was searched.</param>
    /// <returns>A not-found error.</returns>
    [ErrorFactory("USER_NOT_FOUND", ErrorType.NotFound, "User with id '{id}' was not found")]
    public static partial Error UserNotFoundById(int id);
}
