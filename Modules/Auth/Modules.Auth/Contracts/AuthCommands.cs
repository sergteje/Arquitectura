using BuildingBlocks.Abstractions.Mediator;
using ResulTus;

namespace Modules.Auth.Contracts;

/// <summary>Authenticates a user and starts a session.</summary>
/// <param name="Request">The submitted credentials.</param>
public record LoginCommand(LoginRequest Request) : ICommand<Result<AuthResponse>>;

/// <summary>Registers a new user and starts a session.</summary>
/// <param name="Request">The registration payload.</param>
public record RegisterCommand(RegisterRequest Request) : ICommand<Result<AuthResponse>>;

/// <summary>Rotates the access and refresh tokens.</summary>
/// <param name="Request">The refresh payload.</param>
public record RefreshTokenCommand(RefreshTokenRequest Request) : ICommand<Result<RefreshTokenResponse>>;

/// <summary>Ends the session by clearing the stored refresh token.</summary>
/// <param name="UserId">The authenticated user identifier.</param>
public record LogoutCommand(int UserId) : ICommand<Result>;
