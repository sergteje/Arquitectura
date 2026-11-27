using System.Security.Claims;
using Arquitectura.Host.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
        var currentUser = new CurrentUser(AccessorFor(Principal(
            new Claim("sub", "7"),
            new Claim("email", "user@example.com"))));

        currentUser.UserId.Should().Be(7);
        currentUser.Email.Should().Be("user@example.com");
        currentUser.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void RequireUserId_OnAnonymous_Throws()
    {
        var currentUser = new CurrentUser(AccessorFor(new ClaimsPrincipal(new ClaimsIdentity())));

        currentUser.UserId.Should().BeNull();
        currentUser.IsAuthenticated.Should().BeFalse();
        currentUser.Invoking(user => user.RequireUserId())
            .Should().Throw<UnauthorizedAccessException>();
    }
}
