using BuildingBlocks.Abstractions.Mediator;
using Modules.Auth.Contracts;
using Modules.Auth.Domain;
using Modules.Auth.Persistence;
using Modules.Auth.Services;
using ResulTus;

namespace Modules.Auth.Features.Auth.Handlers;

/// <summary>Handles <see cref="LoginCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class LoginCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = await userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);
        if (!userResult.TryGetValue(out var user))
        {
            return Result<AuthResponse>.Failure(
                Error.Unauthorized("AUTH_INVALID_CREDENTIALS", "Invalid email or password"));
        }

        if (!PasswordHasher.Verify(request.Request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(
                Error.Unauthorized("AUTH_INVALID_CREDENTIALS", "Invalid email or password"));
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(
                Error.Forbidden("AUTH_ACCOUNT_DISABLED", "Account is disabled"));
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, RefreshTokenExpiration);
        user.LastLoginAt = DateTime.UtcNow;

        var updateResult = await userRepository.UpdateAsync(user, cancellationToken);
        if (updateResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(
                Error.Failure("AUTH_UPDATE_FAILED", "Failed to update user"));
        }

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(AccessTokenExpiration),
            ToDto(user));

        return Result<AuthResponse>.Success(response);
    }

    internal static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber);
}

/// <summary>Handles <see cref="RegisterCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class RegisterCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existsResult = await userRepository.ExistsAsync(request.Request.Email, cancellationToken);
        if (!existsResult.TryGetValue(out var exists))
        {
            return Result<AuthResponse>.Failure(
                Error.Failure("AUTH_LOOKUP_FAILED", "Error checking user existence"));
        }

        if (exists)
        {
            return Result<AuthResponse>.Failure(
                Error.Conflict("AUTH_EMAIL_EXISTS", "Email already registered"));
        }

        var user = new User
        {
            Email = request.Request.Email,
            PasswordHash = PasswordHasher.Hash(request.Request.Password),
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            CreatedBy = request.Request.Email
        };

        var createResult = await userRepository.CreateAsync(user, cancellationToken);
        if (!createResult.TryGetValue(out user))
        {
            return Result<AuthResponse>.Failure(
                Error.Failure("AUTH_CREATE_FAILED", "Failed to create user"));
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, RefreshTokenExpiration);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(AccessTokenExpiration),
            LoginCommandHandler.ToDto(user));

        return Result<AuthResponse>.Success(response);
    }
}

/// <summary>Handles <see cref="RefreshTokenCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, _) = jwtTokenService.ValidateToken(request.Request.RefreshToken);
        if (userId is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("AUTH_INVALID_REFRESH_TOKEN", "Invalid refresh token"));
        }

        var userResult = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (!userResult.TryGetValue(out var user))
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("AUTH_USER_NOT_FOUND", "User not found"));
        }

        if (!user.IsRefreshTokenValid() || user.RefreshToken != request.Request.RefreshToken)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized("AUTH_REFRESH_TOKEN_EXPIRED", "Refresh token expired"));
        }

        var newAccessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(newRefreshToken, RefreshTokenExpiration);

        var updateResult = await userRepository.UpdateAsync(user, cancellationToken);
        if (updateResult.IsFailure)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Failure("AUTH_UPDATE_FAILED", "Failed to update user"));
        }

        var response = new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.Add(AccessTokenExpiration));

        return Result<RefreshTokenResponse>.Success(response);
    }
}

/// <summary>Handles <see cref="LogoutCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
public sealed class LogoutCommandHandler(IUserRepository userRepository)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.TryGetValue(out var user))
        {
            user.ClearRefreshToken();

            var updateResult = await userRepository.UpdateAsync(user, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure(Error.Failure("AUTH_LOGOUT_FAILED", "Failed to logout"));
            }
        }

        return Result.Success();
    }
}
