namespace Arquitectura.Domain.Entities;

/// <summary>
/// User entity for authentication
/// </summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public bool IsRefreshTokenValid() =>
        RefreshTokenExpiresAt.HasValue && RefreshTokenExpiresAt > DateTime.UtcNow;

    public void UpdateRefreshToken(string token, TimeSpan expiration)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = DateTime.UtcNow.Add(expiration);
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}