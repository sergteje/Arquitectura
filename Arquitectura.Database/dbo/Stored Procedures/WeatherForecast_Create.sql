-- WeatherForecast_Create: Create a new weather forecast
CREATE PROCEDURE dbo.WeatherForecast_Create
    @Date DATETIME,
    @TemperatureC INT,
    @Summary NVARCHAR(500)
AS
BEGIN
    INSERT INTO WeatherForecasts (Date, TemperatureC, Summary)
    VALUES (@Date, @TemperatureC, @Summary);

    SELECT SCOPE_IDENTITY() AS Id;
END