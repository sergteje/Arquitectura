using System.Text;
using BuildingBlocks.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ResulTus;
using ResulTus.AspNetCore;
using Xunit;

namespace Arquitectura.Tests.Host;

public class ResultHttpResultsTests
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

    [Theory]
    [InlineData("NOT_FOUND", 404)]
    [InlineData("VALIDATION_ERROR", 400)]
    [InlineData("RETRY_EXHAUSTED", 503)]
    [InlineData("TIMEOUT", 504)]
    public void Map_LegacyCodes_KeepTheirStatus(string code, int expected)
    {
        ResultStatus.Map(Error.Failure(code, "boom")).Should().Be(expected);
    }

    public static TheoryData<Error, int> ErrorTypeCases => new()
    {
        { Error.Validation("C", "m"), 400 },
        { Error.NotFound("C", "m"), 404 },
        { Error.Conflict("C", "m"), 409 },
        { Error.Unauthorized("C", "m"), 401 },
        { Error.Forbidden("C", "m"), 403 },
        { Error.Timeout("C", "m"), 504 },
        { Error.Cancelled("C", "m"), 499 },
        { Error.Unexpected("C", "m"), 500 },
        { Error.Failure("C", "m"), 400 }
    };

    [Theory]
    [MemberData(nameof(ErrorTypeCases))]
    public void Map_ErrorTypes_MapToExpectedStatus(Error error, int expected)
    {
        ResultStatus.Map(error).Should().Be(expected);
    }

    [Fact]
    public async Task ToProblemHttpResult_RendersRfc9457ProblemDetails()
    {
        var result = Error.NotFound("WEATHERFORECAST_NOT_FOUND", "not here").ToProblemHttpResult();

        var (statusCode, body) = await ExecuteAsync(result);

        statusCode.Should().Be(StatusCodes.Status404NotFound);
        body.Should().Contain("\"status\":404");
        body.Should().Contain("\"detail\":\"not here\"");
        body.Should().Contain("\"code\":\"WEATHERFORECAST_NOT_FOUND\"");
    }

    [Fact]
    public async Task ToHttpResult_OnSuccess_Returns200WithValue()
    {
        var result = Result<int>.Success(42).ToHttpResult(ResultStatus.Map);

        var (statusCode, body) = await ExecuteAsync(result);

        statusCode.Should().Be(StatusCodes.Status200OK);
        body.Should().Be("42");
    }

    [Fact]
    public async Task ToNoContentHttpResult_OnSuccess_Returns204()
    {
        var result = Result.Success().ToNoContentHttpResult();

        var (statusCode, body) = await ExecuteAsync(result);

        statusCode.Should().Be(StatusCodes.Status204NoContent);
        body.Should().BeEmpty();
    }
}
