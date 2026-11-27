using System.Text;
using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ResulTus;
using ResulTus.Validation;
using Xunit;

namespace Arquitectura.Tests.Host;

public class RequestResultHttpResultsTests
{
    private static readonly IServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddOptions()
        .Configure<JsonOptions>(_ => { })
        .BuildServiceProvider();

    private static async Task<(int StatusCode, string Body)> ExecuteAsync(IResult result)
    {
        var context = new DefaultHttpContext { RequestServices = Services };
        var body = new MemoryStream();
        context.Response.Body = body;

        await result.ExecuteAsync(context);

        body.Position = 0;
        using var reader = new StreamReader(body, Encoding.UTF8);
        return (context.Response.StatusCode, await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ToHttpResult_OnSuccess_Returns200WithValue()
    {
        var result = Result<int, RequestError>.Success(42).ToHttpResult();

        var (statusCode, body) = await ExecuteAsync(result);

        statusCode.Should().Be(StatusCodes.Status200OK);
        body.Should().Be("42");
    }

    [Fact]
    public async Task ToNoContentHttpResult_OnSuccess_Returns204()
    {
        var result = Result<Unit, RequestError>.Success(Unit.Value).ToNoContentHttpResult();

        var (statusCode, body) = await ExecuteAsync(result);

        statusCode.Should().Be(StatusCodes.Status204NoContent);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task ToHttpResult_WithValidationErrors_RendersTheErrorsMember()
    {
        var errors = new ValidationErrors([new ValidationIssue("Request.Email", "required", "NotEmpty")]);
        var result = Result<string, RequestError>.Failure(
            new RequestError(Error.Validation("VALIDATION_ERROR", "invalid payload"), errors));

        var (statusCode, body) = await ExecuteAsync(result.ToHttpResult());

        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.Should().Contain("\"errors\"");
        body.Should().Contain("Request.Email");
        body.Should().Contain("required");
    }

    [Fact]
    public async Task ToHttpResult_WithDomainError_RendersProblemDetails()
    {
        var result = Result<string, RequestError>.Failure(
            RequestError.From(Error.NotFound("WEATHERFORECAST_NOT_FOUND", "missing")));

        var (statusCode, body) = await ExecuteAsync(result.ToHttpResult());

        statusCode.Should().Be(StatusCodes.Status404NotFound);
        body.Should().Contain("\"status\":404");
        body.Should().Contain("\"code\":\"WEATHERFORECAST_NOT_FOUND\"");
    }
}
