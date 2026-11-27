using Arquitectura.Host.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResulTus;
using Xunit;

namespace Arquitectura.Tests.Host;

public class ResultHttpExtensionsTests
{
    [Theory]
    [InlineData("NOT_FOUND", 404)]
    [InlineData("VALIDATION_ERROR", 400)]
    [InlineData("RETRY_EXHAUSTED", 503)]
    [InlineData("TIMEOUT", 504)]
    public void StatusCodeFor_LegacyCodes_KeepTheirStatus(string code, int expected)
    {
        var error = Error.Failure(code, "boom");

        ResultHttpExtensions.StatusCodeFor(error).Should().Be(expected);
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
    public void StatusCodeFor_ErrorTypes_MapToExpectedStatus(Error error, int expected)
    {
        ResultHttpExtensions.StatusCodeFor(error).Should().Be(expected);
    }

    [Fact]
    public void ToErrorResult_PreservesCodeMessageAndDetailsShape()
    {
        var error = Error.NotFound("WEATHERFORECAST_NOT_FOUND", "not here");

        var result = ResultHttpExtensions.ToErrorResult(error);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var body = objectResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        body.Code.Should().Be("WEATHERFORECAST_NOT_FOUND");
        body.Message.Should().Be("not here");
        body.Details.Should().BeNull();
    }

    [Fact]
    public void ToActionResult_OnSuccess_Returns200WithValue()
    {
        var result = Result<int>.Success(42);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        objectResult.Value.Should().Be(42);
    }

    [Fact]
    public void ToNoContentResult_OnSuccess_Returns204()
    {
        var result = Result.Success();

        var actionResult = result.ToNoContentResult();

        actionResult.Should().BeOfType<NoContentResult>();
    }
}
