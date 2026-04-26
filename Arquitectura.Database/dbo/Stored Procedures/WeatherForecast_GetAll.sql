-- WeatherForecast_GetAll: Get all weather forecasts with pagination
CREATE PROCEDURE dbo.WeatherForecast_GetAll
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SELECT
        Id,
        Date,
        TemperatureC,
        Summary,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy
    FROM WeatherForecasts
    ORDER BY Id DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1) FROM WeatherForecasts;
END