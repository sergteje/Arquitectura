using Arquitectura.Domain.Entities;

namespace Arquitectura.Domain.Models;

public class WeatherForecastDto : Entity
{
    public required DateOnly Date { get; init; }
    public required int TemperatureC { get; init; }
    public required string Summary { get; init; } = string.Empty;
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record CreateWeatherForecastRequest
{
    public required DateOnly Date { get; init; }
    public required int TemperatureC { get; init; }
    public required string Summary { get; init; } = string.Empty;
}
