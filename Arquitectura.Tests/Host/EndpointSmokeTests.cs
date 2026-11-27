using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Arquitectura.Tests.Host;

public class EndpointSmokeTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;

    public EndpointSmokeTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                // Disable Hangfire (and its schema bootstrap) in tests.
                builder.UseSetting("ConnectionStrings:DefaultConnection", " ");
                builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
            })
            .CreateClient();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Alive_ReturnsOk()
    {
        var response = await _client.GetAsync("/alive", Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiDocument_IsServed()
    {
        var response = await _client.GetAsync("/openapi/v1.json", Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(Token);
        var normalized = content.ToLowerInvariant();
        normalized.Should().Contain("/api/auth/login");
        normalized.Should().Contain("/api/weatherforecast");
        normalized.Should().Contain("/api/datatable/{tablekey}");
    }

    [Fact]
    public async Task ScalarApiReference_IsServed()
    {
        var response = await _client.GetAsync("/scalar", Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task WeatherForecast_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/weatherforecast", Token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsRfc9457ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "nobody@example.com", password = "WrongPass1!" },
            Token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var body = await response.Content.ReadAsStringAsync(Token);
        body.Should().Contain("\"status\":401");
        body.Should().Contain("\"code\"");
        body.Should().Contain("\"detail\"");
    }

    [Fact]
    public async Task Login_WithInvalidPayload_ReturnsValidationProblems()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "not-an-email", password = "" },
            Token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var body = await response.Content.ReadAsStringAsync(Token);
        body.Should().Contain("\"errors\"");
        body.Should().Contain("Request.Email");
    }
}
