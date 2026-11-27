using ResulTus;

namespace Modules.Auth.Services;

/// <summary>The identity carried by a validated access or refresh token.</summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Email">The user e-mail.</param>
public sealed record TokenIdentity(int UserId, string Email);

/// <summary>
/// Issues and validates the tokens used by the Auth module.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Creates a signed access token.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user e-mail.</param>
    /// <param name="role">An optional role claim.</param>
    /// <returns>The serialized JWT.</returns>
    string GenerateAccessToken(int userId, string email, string? role = null);

    /// <summary>Creates an opaque refresh token.</summary>
    /// <returns>The refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>Validates a token and extracts its identity claims.</summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The token identity, or a failure when the token is invalid.</returns>
    Result<TokenIdentity> ValidateToken(string token);
}
