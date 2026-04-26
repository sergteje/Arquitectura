namespace Arquitectura.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string email, string? role = null);
    string GenerateRefreshToken();
    (int? UserId, string? Email) ValidateToken(string token);
}