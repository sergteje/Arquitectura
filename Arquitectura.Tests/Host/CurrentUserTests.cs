using System.Security.Claims;
using Arquitectura.Host.Identity;
using BuildingBlocks.Abstractions.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ResulTus;
using ResulTus.Testing;
using Xunit;

namespace Arquitectura.Tests.Host;

public class CurrentUserTests
{
    private static IHttpContextAccessor AccessorFor(ClaimsPrincipal principal)
    {
        var context = new DefaultHttpContext { User = principal };
        return new HttpContextAccessor { HttpContext = context };
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Test"));

    [Fact]
    public void UserId_ReadsTheSubClaim()
    {
        ICurrentUser currentUser = new CurrentUser(AccessorFor(Principal(
            new Claim("sub", "7"),
            new Claim("email", "user@example.com"))));

        currentUser.UserId.Should().Be(7);
        currentUser.Email.Should().Be("user@example.com");
        currentUser.IsAuthenticated.Should().BeTrue();
        currentUser.UserIdResult.ShouldSucceed().Should().Be(7);
    }

    [Fact]
    public void UserIdResult_OnAnonymous_ReturnsUnauthorized()
    {
        ICurrentUser currentUser = new CurrentUser(AccessorFor(new ClaimsPrincipal(new ClaimsIdentity())));

        currentUser.UserId.Should().BeNull();
        currentUser.IsAuthenticated.Should().BeFalse();
        var error = currentUser.UserIdResult.ShouldFail();
        error.Type.Should().Be(ErrorType.Unauthorized);
        error.Code.Should().Be("AUTH_REQUIRED");
    }
}
