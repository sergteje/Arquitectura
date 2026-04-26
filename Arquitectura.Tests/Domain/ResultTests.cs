using Arquitectura.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Arquitectura.Tests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnValue_WithNoError()
    {
        // Arrange & Act
        var result = Result<string>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldReturnError_WithNoValue()
    {
        // Arrange & Act
        var error = ResultError.Create("NOT_FOUND", "Not found");
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void Match_WhenSuccess_ShouldCallOnSuccess()
    {
        // Arrange
        var result = Result<string>.Success("hello");

        // Act
        var output = result.Match(
            onSuccess: value => value.ToUpper(),
            onFailure: error => "error");

        // Assert
        output.Should().Be("HELLO");
    }

    [Fact]
    public void Match_WhenFailure_ShouldCallOnFailure()
    {
        // Arrange
        var result = Result<string>.Failure("NOT_FOUND", "Not found");

        // Act
        var output = result.Match(
            onSuccess: value => value.ToUpper(),
            onFailure: error => error.Code);

        // Assert
        output.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void Result_WithValue_TypeShouldBeInferred()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

public class ResultErrorTests
{
    [Fact]
    public void Create_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var error = ResultError.Create("TEST_CODE", "Test message");

        // Assert
        error.Code.Should().Be("TEST_CODE");
        error.Message.Should().Be("Test message");
    }

    [Fact]
    public void Create_WithDetails_ShouldIncludeDetails()
    {
        // Arrange
        var details = new Dictionary<string, object> { ["id"] = 1 };

        // Act
        var error = ResultError.Create("VALIDATION_ERROR", "Invalid", details);

        // Assert
        error.Details.Should().NotBeNull();
        error.Details!["id"].Should().Be(1);
    }
}

public class ServiceErrorsTests
{
    [Fact]
    public void NotFound_ShouldCreateCorrectError()
    {
        // Arrange & Act
        var error = ServiceErrors.NotFound("User", "123");

        // Assert
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Contain("User");
        error.Details!["entityId"].Should().Be("123");
    }

    [Fact]
    public void ValidationError_ShouldCreateCorrectError()
    {
        // Arrange & Act
        var error = ServiceErrors.ValidationError("Name is required");

        // Assert
        error.Code.Should().Be("VALIDATION_ERROR");
        error.Message.Should().Be("Name is required");
    }

    [Fact]
    public void RetryExhausted_ShouldCreateCorrectError()
    {
        // Arrange & Act
        var error = ServiceErrors.RetryExhausted(3, "Timeout");

        // Assert
        error.Code.Should().Be("RETRY_EXHAUSTED");
        error.Details!["attempts"].Should().Be(3);
    }
}