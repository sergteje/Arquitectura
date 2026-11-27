using BuildingBlocks.Abstractions.Mediator;

namespace Modules.Auth.Contracts;

/// <summary>Authenticates a user and starts a session.</summary>
/// <param name="Request">The submitted credentials.</param>
public record LoginCommand(LoginRequest Request) : ICommand<AuthResponse>;

/// <summary>Registers a new user and starts a session.</summary>
/// <param name="Request">The registration payload.</param>
public record RegisterCommand(RegisterRequest Request) : ICommand<AuthResponse>;

/// <summary>Rotates the access and refresh tokens.</summary>
/// <param name="Request">The refresh payload.</param>
public record RefreshTokenCommand(RefreshTokenRequest Request) : ICommand<RefreshTokenResponse>;

/// <summary>Ends the session by clearing the stored refresh token.</summary>
/// <param name="UserId">The authenticated user identifier.</param>
public record LogoutCommand(int UserId) : ICommand<Unit>;
