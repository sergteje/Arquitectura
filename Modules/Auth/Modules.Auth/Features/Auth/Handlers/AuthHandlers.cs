using BuildingBlocks.Abstractions.Mediator;
using Modules.Auth.Contracts;
using Modules.Auth.Domain;
using Modules.Auth.Errors;
using Modules.Auth.Persistence;
using Modules.Auth.Services;
using ResulTus;
using ResulTus.Extensions;

namespace Modules.Auth.Features.Auth.Handlers;

/// <summary>Handles <see cref="LoginCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<AuthResponse, RequestError>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await userRepository.GetByEmailAsync(request.Request.Email, cancellationToken)
            .MapErrorAsync(
                (_, _) => ValueTask.FromResult(AuthErrors.InvalidCredentials()),
                cancellationToken)
            .EnsureAsync(
                (user, _) => ValueTask.FromResult(
                    PasswordHasher.Verify(request.Request.Password, user.PasswordHash)),
                AuthErrors.InvalidCredentials(),
                cancellationToken)
            .EnsureAsync(
                (user, _) => ValueTask.FromResult(user.IsActive),
                AuthErrors.AccountDisabled(),
                cancellationToken)
            .BindAsync(
                (user, token) => UpdateAndRespond(user, request.Request.Password, token),
                cancellationToken);

        return result.ToRequestResult();
    }

    private async ValueTask<Result<AuthResponse>> UpdateAndRespond(
        User user,
        string password,
        CancellationToken cancellationToken)
    {
        if (PasswordHasher.NeedsRehash(user.PasswordHash))
        {
            user.PasswordHash = PasswordHasher.Hash(password);
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, RefreshTokenExpiration);
        user.LastLoginAt = DateTime.UtcNow;

        return (await userRepository.UpdateAsync(user, cancellationToken))
            .MapError(_ => AuthErrors.UpdateFailed())
            .Map(_ => new AuthResponse(
                accessToken,
                refreshToken,
                DateTime.UtcNow.Add(AccessTokenExpiration),
                ToDto(user)));
    }

    internal static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber);
}

/// <summary>Handles <see cref="RegisterCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class RegisterCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<AuthResponse, RequestError>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var result = await userRepository.ExistsAsync(request.Request.Email, cancellationToken)
            .MapErrorAsync(
                (_, _) => ValueTask.FromResult(AuthErrors.LookupFailed()),
                cancellationToken)
            .EnsureAsync(
                (exists, _) => ValueTask.FromResult(!exists),
                AuthErrors.EmailExists(),
                cancellationToken)
            .BindAsync(
                (_, token) => CreateAndRespond(request, token),
                cancellationToken);

        return result.ToRequestResult();
    }

    private async ValueTask<Result<AuthResponse>> CreateAndRespond(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Request.Email,
            PasswordHash = PasswordHasher.Hash(request.Request.Password),
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            CreatedBy = request.Request.Email
        };

        return (await userRepository.CreateAsync(user, cancellationToken))
            .MapError(_ => AuthErrors.CreateFailed())
            .Map(BuildResponse);
    }

    private AuthResponse BuildResponse(User user)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, RefreshTokenExpiration);

        return new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(AccessTokenExpiration),
            LoginCommandHandler.ToDto(user));
    }
}

/// <summary>Handles <see cref="RefreshTokenCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
/// <param name="jwtTokenService">The token service.</param>
public sealed class RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private static readonly TimeSpan AccessTokenExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

    /// <inheritdoc />
    public async Task<Result<RefreshTokenResponse, RequestError>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var result = await Task.FromResult(jwtTokenService.ValidateToken(request.Request.RefreshToken))
            .BindAsync(
                (identity, token) => LoadAndRotate(identity, request.Request.RefreshToken, token),
                cancellationToken);

        return result.ToRequestResult();
    }

    private async ValueTask<Result<RefreshTokenResponse>> LoadAndRotate(
        TokenIdentity identity,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var userResult = (await userRepository.GetByIdAsync(identity.UserId, cancellationToken))
            .MapError(_ => AuthErrors.UserNotFound());

        return await Task.FromResult(userResult)
            .EnsureAsync(
                (user, _) => ValueTask.FromResult(
                    user.IsRefreshTokenValid() && user.RefreshToken == refreshToken),
                AuthErrors.RefreshTokenExpired(),
                cancellationToken)
            .BindAsync((user, token) => Rotate(user, token), cancellationToken);
    }

    private async ValueTask<Result<RefreshTokenResponse>> Rotate(
        User user,
        CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, RefreshTokenExpiration);

        return (await userRepository.UpdateAsync(user, cancellationToken))
            .MapError(_ => AuthErrors.UpdateFailed())
            .Map(_ => new RefreshTokenResponse(
                accessToken,
                refreshToken,
                DateTime.UtcNow.Add(AccessTokenExpiration)));
    }
}

/// <summary>Handles <see cref="LogoutCommand"/>.</summary>
/// <param name="userRepository">The user repository.</param>
public sealed class LogoutCommandHandler(IUserRepository userRepository)
    : IRequestHandler<LogoutCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Result<Unit, RequestError>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var userResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        var result = await userResult.Match(
            user => ClearAndPersist(user, cancellationToken),
            _ => Task.FromResult(Result.Success()));

        return result.ToUnitRequestResult();
    }

    private async Task<Result> ClearAndPersist(User user, CancellationToken cancellationToken)
    {
        user.ClearRefreshToken();

        return (await userRepository.UpdateAsync(user, cancellationToken))
            .MapError(_ => AuthErrors.LogoutFailed())
            .ToUnitResult();
    }
}
