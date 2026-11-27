using BuildingBlocks.Abstractions.Domain;

namespace Modules.Auth.Domain;

/// <summary>
/// A registered account.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Gets or sets the account e-mail (unique).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the password hash.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether the e-mail was confirmed.</summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>Gets or sets a value indicating whether the account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the last successful login instant (UTC).</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Gets or sets the current refresh token.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>Gets or sets the instant the refresh token expires (UTC).</summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>Gets the display name.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Checks whether the stored refresh token is still valid.
    /// </summary>
    /// <returns><c>true</c> when a non-expired refresh token is present.</returns>
    public bool IsRefreshTokenValid() =>
        RefreshTokenExpiresAt.HasValue && RefreshTokenExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Replaces the refresh token and its expiration.
    /// </summary>
    /// <param name="token">The new refresh token.</param>
    /// <param name="expiration">How long the token stays valid.</param>
    public void UpdateRefreshToken(string token, TimeSpan expiration)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = DateTime.UtcNow.Add(expiration);
    }

    /// <summary>Clears the stored refresh token.</summary>
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}
