using Arquitectura.Host.Web.Middleware;
using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Host;

public class LogBodyRedactorTests
{
    [Fact]
    public void Redact_OnLoginBody_MasksPasswordAndKeepsEmail()
    {
        var body = """{"email":"user@example.com","password":"SuperSecret1!"}""";

        var redacted = LogBodyRedactor.Redact(body);

        redacted.Should().Contain("user@example.com");
        redacted.Should().Contain(LogBodyRedactor.RedactedValue);
        redacted.Should().NotContain("SuperSecret1!");
    }

    [Fact]
    public void Redact_OnTokenResponse_MasksEveryTokenShape()
    {
        var body = """
                   {
                     "accessToken": "header.payload.signature",
                     "refreshToken": "refresh-value",
                     "user": { "email": "user@example.com" }
                   }
                   """;

        var redacted = LogBodyRedactor.Redact(body);

        redacted.Should().NotContain("header.payload.signature");
        redacted.Should().NotContain("refresh-value");
        redacted.Should().Contain("user@example.com");
    }

    [Fact]
    public void Redact_InsideArraysAndNestedObjects_MasksTheMemberNameNotThePosition()
    {
        var body = """{"items":[{"name":"a","secret":"s1"},{"name":"b","token":"t2"}]}""";

        var redacted = LogBodyRedactor.Redact(body);

        redacted.Should().Contain("\"name\":\"a\"");
        redacted.Should().Contain("\"name\":\"b\"");
        redacted.Should().NotContain("s1");
        redacted.Should().NotContain("t2");
    }

    [Fact]
    public void Redact_OnNonJsonBody_ReturnsThePlaceholder()
    {
        var redacted = LogBodyRedactor.Redact("--boundary\r\npassword=SuperSecret1!");

        redacted.Should().Be(LogBodyRedactor.NonJsonPlaceholder);
        redacted.Should().NotContain("SuperSecret1!");
    }

    [Fact]
    public void Redact_OnEmptyBody_ReturnsEmpty()
    {
        LogBodyRedactor.Redact(null).Should().BeEmpty();
        LogBodyRedactor.Redact("   ").Should().BeEmpty();
    }
}
