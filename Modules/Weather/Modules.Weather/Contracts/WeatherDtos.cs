namespace Modules.Weather.Contracts;

/// <summary>
/// A weather forecast as exposed over HTTP.
/// </summary>
public sealed class WeatherForecastDto
{
    /// <summary>Gets or sets the identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the forecast date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Gets or sets the temperature in Celsius.</summary>
    public int TemperatureC { get; set; }

    /// <summary>Gets or sets the summary text.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets the temperature in Fahrenheit.</summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

/// <summary>
/// Payload used to create or update a forecast.
/// </summary>
public sealed class CreateWeatherForecastRequest
{
    /// <summary>Gets or sets the forecast date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Gets or sets the temperature in Celsius.</summary>
    public int TemperatureC { get; set; }

    /// <summary>Gets or sets the summary text.</summary>
    public string Summary { get; set; } = string.Empty;
}
