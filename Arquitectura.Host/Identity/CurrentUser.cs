using System.Security.Claims;
using BuildingBlocks.Abstractions.Identity;
using Microsoft.AspNetCore.Http;

namespace Arquitectura.Host.Identity;

/// <summary>
/// Resolves the authenticated caller from the current HTTP request's JWT claims.
/// </summary>
/// <remarks>
/// The JWT issuer writes the user identifier to the <c>sub</c> claim, so this is the primary
/// source. <see cref="ClaimTypes.NameIdentifier"/> is kept as a fallback for other issuers.
/// </remarks>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public int? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue("sub")
                      ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string? Email =>
        Principal?.FindFirstValue("email")
        ?? Principal?.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc />
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
}
