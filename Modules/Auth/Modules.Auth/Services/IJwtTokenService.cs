namespace Modules.Auth.Services;

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
    /// <returns>The user identifier and e-mail, or <c>(null, null)</c> when invalid.</returns>
    (int? UserId, string? Email) ValidateToken(string token);
}
