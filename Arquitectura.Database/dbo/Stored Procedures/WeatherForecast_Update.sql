-- WeatherForecast_Update: Update an existing weather forecast
CREATE PROCEDURE dbo.WeatherForecast_Update
    @Id INT,
    @Date DATETIME,
    @TemperatureC INT,
    @Summary NVARCHAR(500)
AS
BEGIN
    UPDATE WeatherForecasts
    SET
        Date = @Date,
        TemperatureC = @TemperatureC,
        Summary = @Summary,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END