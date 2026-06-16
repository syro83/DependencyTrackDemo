namespace WeatherApiService.Api.WeatherCity;

static class WeatherEndpoints
{
    // Marker type used solely as the ILogger category name — static classes cannot be type arguments.
    private sealed class Log;

    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weatherforecast", async (IWeatherCityService weatherCityService, ILogger<Log> logger, CancellationToken ct) =>
        {
            logger.LogInformation("GET /weatherforecast — picking a random city");
            var forecast = await weatherCityService.GetRandomCityForecastAsync(ct);
            if (forecast is null)
            {
                logger.LogWarning("Failed to get forecast for random city");
                return Results.Problem("Could not fetch weather for a random city");
            }
            logger.LogInformation("Returning forecast for {City}, {Country}", forecast.City, forecast.Country);
            return Results.Ok(forecast);
        })
        .WithName("GetWeatherForecast");  
              
        app.MapGet("/weatherforecast/{city}", async (
            string city,
            IWeatherCityService weatherCityService,
            ILogger<Log> logger,
            CancellationToken cancellationToken) =>
        {
            logger.LogInformation("GET /weatherforecast/{City} request received", city);

            var forecast = await weatherCityService.GetForecastAsync(city, cancellationToken);

            if (forecast is null)
            {
                logger.LogWarning("No forecast available for city {City}", city);
                return Results.NotFound(new { error = $"City '{city}' not found or has no forecast data." });
            }

            logger.LogInformation(
                "Returning {Count}-day forecast for {City}, {Country}",
                forecast.Forecast.Count, forecast.City, forecast.Country);

            return Results.Ok(forecast);
        })
        .WithName("GetCityWeatherForecast");

        app.MapGet("/weatherforecast/{city}/hourly", async (
            string city,
            IWeatherCityService weatherCityService,
            ILogger<Log> logger,
            CancellationToken cancellationToken) =>
        {
            logger.LogInformation("GET /weatherforecast/{City}/hourly request received", city);
            var hourly = await weatherCityService.GetHourlyForecastAsync(city, cancellationToken);
            if (hourly is null)
            {
                logger.LogWarning("No hourly forecast available for city {City}", city);
                return Results.NotFound(new { error = $"City '{city}' not found or has no hourly data." });
            }
            return Results.Ok(hourly);
        })
        .WithName("GetCityHourlyForecast");

        return app;
    }
}
