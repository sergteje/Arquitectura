using BuildingBlocks.Abstractions.Mediator;
using BuildingBlocks.Infrastructure.Behaviors;
using FluentAssertions;
using FluentValidation;
using ResulTus;
using ResulTus.Testing;
using Xunit;

namespace Arquitectura.Tests.Mediator;

public class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Name) : IRequest<string>;

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator() => RuleFor(request => request.Name).NotEmpty();
    }

    [Fact]
    public async Task Handle_WhenARuleFails_ReturnsValidationFailureWithoutCallingNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new SampleRequest(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(Result<string, RequestError>.Success("ok"));
            },
            TestContext.Current.CancellationToken);

        var requestError = result.ShouldFail();
        requestError.Error.Type.Should().Be(ErrorType.Validation);
        requestError.ValidationErrors.Should().NotBeNull();
        requestError.ValidationErrors!.Issues.Should().NotBeEmpty();
        requestError.ValidationErrors.Issues[0].Field.Should().Contain("Name");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenRulesPass_ContinuesThePipeline()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);

        var result = await behavior.Handle(
            new SampleRequest("Ada"),
            _ => Task.FromResult(Result<string, RequestError>.Success("ok")),
            TestContext.Current.CancellationToken);

        result.ShouldSucceed().Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithNoValidator_ContinuesThePipeline()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var result = await behavior.Handle(
            new SampleRequest(string.Empty),
            _ => Task.FromResult(Result<string, RequestError>.Success("ok")),
            TestContext.Current.CancellationToken);

        result.ShouldSucceed().Should().Be("ok");
    }
}
