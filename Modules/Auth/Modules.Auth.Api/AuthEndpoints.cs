using BuildingBlocks.Abstractions.Identity;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.AspNetCore;
using ResulTus.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Modules.Auth.Contracts;

namespace Modules.Auth.Api;

/// <summary>Minimal API endpoints owned by the Auth module.</summary>
public static class AuthEndpoints
{
    /// <summary>Maps the Auth module endpoints.</summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The same endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireRateLimiting("fixed");

        group.MapPost("/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
                (await sender.Send(new LoginCommand(request), cancellationToken)).ToHttpResult(ResultStatus.Map))
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/register", async (
                RegisterRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new RegisterCommand(request), cancellationToken);

                return result.Match<IResult>(
                    value => Results.Json(value, statusCode: StatusCodes.Status201Created),
                    error => error.ToProblemHttpResult());
            })
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/refresh", async (
                RefreshTokenRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
                (await sender.Send(new RefreshTokenCommand(request), cancellationToken)).ToHttpResult(ResultStatus.Map))
            .RequireAuthorization()
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/logout", async (
                ICurrentUser currentUser,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is { } userId)
                {
                    var result = await sender.Send(new LogoutCommand(userId), cancellationToken);
                    return result.ToNoContentHttpResult();
                }

                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
