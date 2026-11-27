using ResulTus;

namespace Modules.Weather.Errors;

/// <summary>Errors owned by the Weather module.</summary>
[ResultErrors]
public static partial class WeatherErrors
{
    /// <summary>No forecast matches the identifier.</summary>
    /// <param name="id">The identifier that was searched.</param>
    /// <returns>A not-found error.</returns>
    [ErrorFactory("WEATHERFORECAST_NOT_FOUND", ErrorType.NotFound, "Weather forecast with id '{id}' was not found")]
    public static partial Error ForecastNotFound(int id);
}
