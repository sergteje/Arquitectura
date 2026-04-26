using Arquitectura.Application.Features.Auth.DTO;
using Arquitectura.Domain.Models;
using MediatR;

namespace Arquitectura.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<Result<AuthResponse>>;
public record RegisterCommand(RegisterRequest Request) : IRequest<Result<AuthResponse>>;
public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<Result<RefreshTokenResponse>>;
public record LogoutCommand(int UserId) : IRequest<Result>;