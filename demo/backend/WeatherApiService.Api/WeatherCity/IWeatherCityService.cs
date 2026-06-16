namespace WeatherApiService.Api.WeatherCity;

interface IWeatherCityService
{
    /// <summary>
    /// Returns a 5-day forecast for the given city name, or <c>null</c> if the city cannot be resolved.
    /// </summary>
    Task<CityForecastResponse?> GetForecastAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a 5-day forecast for a randomly chosen city from the known city list.
    /// </summary>
    Task<CityForecastResponse?> GetRandomCityForecastAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns 24-hour hourly forecast for today for the given city, or <c>null</c> if the city cannot be resolved.
    /// </summary>
    Task<CityHourlyForecastResponse?> GetHourlyForecastAsync(string city, CancellationToken cancellationToken = default);
}
