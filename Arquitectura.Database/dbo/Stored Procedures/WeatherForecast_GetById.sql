-- WeatherForecast_GetById: Get a single weather forecast by ID
CREATE PROCEDURE dbo.WeatherForecast_GetById
    @Id INT
AS
BEGIN
    SELECT
        Id,
        Date,
        TemperatureC,
        Summary
    FROM WeatherForecasts
    WHERE Id = @Id;
END