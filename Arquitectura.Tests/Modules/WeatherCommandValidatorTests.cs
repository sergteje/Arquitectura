using FluentAssertions;
using FluentValidation.TestHelper;
using Modules.Weather.Contracts;
using Modules.Weather.Features.Weather.Validators;
using Xunit;

namespace Arquitectura.Tests.Modules;

public class WeatherCommandValidatorTests
{
    private readonly CreateWeatherForecastCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var command = new CreateWeatherForecastCommand(new CreateWeatherForecastRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TemperatureC = 20,
            Summary = "Warm"
        });

        _validator.TestValidate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void PastDate_Fails()
    {
        var command = new CreateWeatherForecastCommand(new CreateWeatherForecastRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            TemperatureC = 20,
            Summary = "Warm"
        });

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Request.Date);
    }

    [Fact]
    public void OutOfRangeTemperature_Fails()
    {
        var command = new CreateWeatherForecastCommand(new CreateWeatherForecastRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TemperatureC = 500,
            Summary = "Hot"
        });

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Request.TemperatureC);
    }

    [Fact]
    public void EmptySummary_Fails()
    {
        var command = new CreateWeatherForecastCommand(new CreateWeatherForecastRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TemperatureC = 20,
            Summary = string.Empty
        });

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Request.Summary);
    }
}
