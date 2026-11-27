using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Moq;
using Modules.Auth.Contracts;
using Modules.Auth.Domain;
using Modules.Auth.Features.Auth.Handlers;
using Modules.Auth.Persistence;
using Modules.Auth.Services;
using ResulTus;
using Xunit;

namespace Arquitectura.Tests.Modules;

public class LoginCommandHandlerTests
{
    private static string Hash(string password) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    private static User ActiveUser(string password) => new()
    {
        Id = 1,
        Email = "user@example.com",
        PasswordHash = Hash(password),
        FirstName = "Ada",
        LastName = "Lovelace",
        IsActive = true
    };

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSession()
    {
        var user = ActiveUser("Password1!");

        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));
        repository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        var tokens = new Mock<IJwtTokenService>();
        tokens.Setup(x => x.GenerateAccessToken(user.Id, user.Email, It.IsAny<string?>()))
            .Returns("access-token");
        tokens.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

        var handler = new LoginCommandHandler(repository.Object, tokens.Object);

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest(user.Email, "Password1!")),
            CancellationToken.None);

        result.TryGetValue(out var response).Should().BeTrue();
        response!.AccessToken.Should().Be("access-token");
        response!.RefreshToken.Should().Be("refresh-token");
        response!.User.Email.Should().Be(user.Email);
        user.PasswordHash.Should().StartWith("PBKDF2$", "the legacy hash is upgraded on successful login");
        repository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = ActiveUser("Password1!");

        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        var handler = new LoginCommandHandler(repository.Object, Mock.Of<IJwtTokenService>());

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest(user.Email, "wrong-password")),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.TryGetValue(out _, out var error).Should().BeFalse();
        error.Code.Should().Be("AUTH_INVALID_CREDENTIALS");
        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure(Error.NotFound("USER_NOT_FOUND", "missing")));

        var handler = new LoginCommandHandler(repository.Object, Mock.Of<IJwtTokenService>());

        var result = await handler.Handle(
            new LoginCommand(new LoginRequest("ghost@example.com", "Password1!")),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.TryGetValue(out _, out var error).Should().BeFalse();
        error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
