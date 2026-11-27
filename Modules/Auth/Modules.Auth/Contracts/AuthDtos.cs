namespace Modules.Auth.Contracts;

/// <summary>Credentials submitted to the login endpoint.</summary>
/// <param name="Email">The account e-mail.</param>
/// <param name="Password">The plain-text password.</param>
public record LoginRequest(string Email, string Password);

/// <summary>Registration payload.</summary>
/// <param name="Email">The account e-mail.</param>
/// <param name="Password">The plain-text password.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

/// <summary>Refresh-token payload.</summary>
/// <param name="RefreshToken">The refresh token previously issued.</param>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>Authenticated session returned after login or registration.</summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="RefreshToken">The opaque refresh token.</param>
/// <param name="ExpiresAt">The instant the access token expires (UTC).</param>
/// <param name="User">The authenticated user.</param>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

/// <summary>Public projection of an authenticated user.</summary>
/// <param name="Id">The user identifier.</param>
/// <param name="Email">The e-mail.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="PhoneNumber">The optional phone number.</param>
public record UserDto(int Id, string Email, string FirstName, string LastName, string? PhoneNumber);

/// <summary>Rotated token pair returned by the refresh endpoint.</summary>
/// <param name="AccessToken">The new signed JWT access token.</param>
/// <param name="RefreshToken">The new refresh token.</param>
/// <param name="ExpiresAt">The instant the access token expires (UTC).</param>
public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
