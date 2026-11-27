using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Modules.Auth.Features.Auth;
using Xunit;

namespace Arquitectura.Tests.Modules;

public class PasswordHasherTests
{
    private static string LegacySha256(string password) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    [Fact]
    public void Hash_ProducesSaltedPbkdf2Hashes()
    {
        var first = PasswordHasher.Hash("Password1!");
        var second = PasswordHasher.Hash("Password1!");

        first.Should().StartWith("PBKDF2$");
        first.Should().NotBe(second, "each hash uses a random salt");
    }

    [Fact]
    public void Verify_AcceptsTheGeneratedHash()
    {
        var hash = PasswordHasher.Hash("Password1!");

        PasswordHasher.Verify("Password1!", hash).Should().BeTrue();
        PasswordHasher.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_AcceptsLegacySha256()
    {
        var legacy = LegacySha256("Password1!");

        PasswordHasher.Verify("Password1!", legacy).Should().BeTrue();
        PasswordHasher.Verify("wrong", legacy).Should().BeFalse();
    }

    [Fact]
    public void NeedsRehash_IsTrueForLegacyAndFalseForCurrent()
    {
        PasswordHasher.NeedsRehash(LegacySha256("Password1!")).Should().BeTrue();
        PasswordHasher.NeedsRehash(PasswordHasher.Hash("Password1!")).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("PBKDF2$bad$parts")]
    public void Verify_WithMalformedHash_ReturnsFalse(string hash)
    {
        PasswordHasher.Verify("Password1!", hash).Should().BeFalse();
    }
}
