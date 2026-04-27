using Arquitectura.Application.Features.Auth.Commands;
using Arquitectura.Application.Features.Auth.DTO;
using Arquitectura.Application.Interfaces;
using Arquitectura.Domain.Entities;
using Arquitectura.Domain.Models;
using Arquitectura.Persistence.Repositories;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Arquitectura.Application.Features.Auth.Handlers;

public class AuthHandler :
    IRequestHandler<LoginCommand, Result<AuthResponse>>,
    IRequestHandler<RegisterCommand, Result<AuthResponse>>,
    IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>,
    IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly TimeSpan _accessTokenExpiration = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _refreshTokenExpiration = TimeSpan.FromDays(7);

    public AuthHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);
        
        if (userResult.IsFailure)
            return Result<AuthResponse>.Failure("AUTH", "User not found");

        var user = userResult.Value!;

        // Check password
        if (!VerifyPassword(request.Request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("AUTH", "Invalid email or password");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("AUTH", "Account is disabled");

        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, _refreshTokenExpiration);
        user.LastLoginAt = DateTime.UtcNow;
        
        var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);
        if (updateResult.IsFailure)
            return Result<AuthResponse>.Failure("AUTH", "Failed to update user");

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(_accessTokenExpiration),
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber));

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existsResult = await _userRepository.ExistsAsync(request.Request.Email, cancellationToken);
        
        if (existsResult.IsFailure)
            return Result<AuthResponse>.Failure("AUTH", "Error checking user existence");

        if (existsResult.Value)
            return Result<AuthResponse>.Failure("AUTH", "Email already registered");

        var user = new User
        {
            Email = request.Request.Email,
            PasswordHash = HashPassword(request.Request.Password),
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            CreatedBy = request.Request.Email
        };

        var createResult = await _userRepository.CreateAsync(user, cancellationToken);
        
        if (createResult.IsFailure)
            return Result<AuthResponse>.Failure("AUTH", createResult.Error!.Message);

        user = createResult.Value!;

        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(refreshToken, _refreshTokenExpiration);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(_accessTokenExpiration),
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber));

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (userId, email) = _jwtTokenService.ValidateToken(request.Request.RefreshToken);
        if (userId == null || email == null)
            return Result<RefreshTokenResponse>.Failure("AUTH", "Invalid refresh token");

        var userResult = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        
        if (userResult.IsFailure)
            return Result<RefreshTokenResponse>.Failure("AUTH", "User not found");

        var user = userResult.Value!;

        if (!user.IsRefreshTokenValid() || user.RefreshToken != request.Request.RefreshToken)
            return Result<RefreshTokenResponse>.Failure("AUTH", "Refresh token expired");

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.UpdateRefreshToken(newRefreshToken, _refreshTokenExpiration);
        
        var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);
        if (updateResult.IsFailure)
            return Result<RefreshTokenResponse>.Failure("AUTH", "Failed to update user");

        var response = new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.Add(_accessTokenExpiration));

        return Result<RefreshTokenResponse>.Success(response);
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (userResult.IsSuccess)
        {
            var user = userResult.Value!;
            user.ClearRefreshToken();
            
            var updateResult = await _userRepository.UpdateAsync(user, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Failure("AUTH", "Failed to logout");
        }
        
        return Result.Success();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }
}