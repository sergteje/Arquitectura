using Arquitectura.Host.Identity;
using Arquitectura.Host.Web;
using BuildingBlocks.Abstractions.Identity;
using BuildingBlocks.Abstractions.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Modules.Auth.Contracts;

namespace Arquitectura.Host.Controllers;

/// <summary>Authentication endpoints.</summary>
/// <param name="sender">The mediator.</param>
/// <param name="currentUser">The current user resolver.</param>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
[EnableRateLimiting("fixed")]
public sealed class AuthController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Authenticates a user.</summary>
    /// <param name="request">The submitted credentials.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the session, or a mapped error.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginCommand(request), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Registers a new user.</summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>201 with the session, or a mapped error.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCommand(request), cancellationToken);
        return result.Match<IActionResult>(
            value => StatusCode(StatusCodes.Status201Created, value),
            ResultHttpExtensions.ToErrorResult);
    }

    /// <summary>Rotates the access and refresh tokens.</summary>
    /// <param name="request">The refresh payload.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>200 with the rotated tokens, or a mapped error.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [Authorize]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshTokenCommand(request), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Ends the session.</summary>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>204.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is { } userId)
        {
            await sender.Send(new LogoutCommand(userId), cancellationToken);
        }

        return NoContent();
    }
}
