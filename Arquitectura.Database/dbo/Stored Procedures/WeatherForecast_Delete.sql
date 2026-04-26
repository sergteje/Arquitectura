-- WeatherForecast_Delete: Delete a weather forecast by ID
CREATE PROCEDURE dbo.WeatherForecast_Delete
    @Id INT
AS
BEGIN
    DELETE FROM WeatherForecasts
    WHERE Id = @Id;
END