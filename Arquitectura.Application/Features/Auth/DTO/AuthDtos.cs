namespace Arquitectura.Application.Features.Auth.DTO;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(int Id, string Email, string FirstName, string LastName, string? PhoneNumber);

public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);