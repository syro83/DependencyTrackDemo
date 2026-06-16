using System.Globalization;
using System.Net.Http.Json;
using WeatherApiService.Api.Cities;
using WeatherApiService.Api.WeatherCity.OpenMeteo;
using WeatherApiService.Api.WeatherImages;

namespace WeatherApiService.Api.WeatherCity;

sealed class WeatherCityService(
    HttpClient httpClient,
    CitiesRepository citiesRepository,
    WeatherImagesRepository imagesRepository,
    ILogger<WeatherCityService> logger) : IWeatherCityService
{
    private const string GeocodingUrl =
        "https://geocoding-api.open-meteo.com/v1/search?name={0}&count=1&language=en&format=json";

    private const string ForecastUrl =
        "https://api.open-meteo.com/v1/forecast" +
        "?latitude={0}&longitude={1}" +
        "&daily=temperature_2m_max,temperature_2m_min,weathercode" +
        ",precipitation_sum,wind_speed_10m_max,uv_index_max,sunrise,sunset" +
        ",sunshine_duration,precipitation_probability_max" +
        "&timezone=auto&forecast_days=5";

    private const string HourlyUrl =
        "https://api.open-meteo.com/v1/forecast" +
        "?latitude={0}&longitude={1}" +
        "&hourly=temperature_2m,precipitation_probability,weathercode,wind_speed_10m" +
        "&timezone=auto&forecast_hours=24";

    public async Task<CityForecastResponse?> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        var knownCity = citiesRepository.All
            .FirstOrDefault(c => string.Equals(c.Name, city, StringComparison.OrdinalIgnoreCase));

        GeocodingResult? location;
        if (knownCity is not null)
        {
            logger.LogInformation("City {City} found in repository — skipping geocoding", city);
            location = new GeocodingResult(
                Name: knownCity.Name,
                Latitude: knownCity.Lat,
                Longitude: knownCity.Lon,
                Country: knownCity.Country,
                CountryCode: string.Empty);
        }
        else
        {
            location = await ResolveLocationAsync(city, cancellationToken);
            if (location is null)
                return null;
        }

        var forecast = await FetchForecastAsync(location, cancellationToken);
        if (forecast is null)
            return null;

        return WeatherCityMapper.ToResponse(
            location,
            forecast,
            region: knownCity?.Region ?? string.Empty,
            cityImageUrl: knownCity?.ImageUrl ?? string.Empty,
            imagesRepository);
    }

    public async Task<CityForecastResponse?> GetRandomCityForecastAsync(CancellationToken cancellationToken = default)
    {
        var all = citiesRepository.All;
        var randomCity = all[Random.Shared.Next(all.Count)];
        logger.LogInformation("Random city selected: {City}", randomCity.Name);
        return await GetForecastAsync(randomCity.Name, cancellationToken);
    }

    public async Task<CityHourlyForecastResponse?> GetHourlyForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        var knownCity = citiesRepository.All
            .FirstOrDefault(c => string.Equals(c.Name, city, StringComparison.OrdinalIgnoreCase));

        GeocodingResult? location;
        if (knownCity is not null)
        {
            location = new GeocodingResult(
                Name: knownCity.Name,
                Latitude: knownCity.Lat,
                Longitude: knownCity.Lon,
                Country: knownCity.Country,
                CountryCode: string.Empty);
        }
        else
        {
            location = await ResolveLocationAsync(city, cancellationToken);
            if (location is null)
                return null;
        }

        var url = string.Format(CultureInfo.InvariantCulture, HourlyUrl, location.Latitude, location.Longitude);
        logger.LogInformation("Fetching hourly forecast for {City}", location.Name);

        try
        {
            var response = await httpClient.GetFromJsonAsync<HourlyForecastApiResponse>(url, cancellationToken);
            if (response is null)
                return null;

            return WeatherCityMapper.ToHourlyResponse(location.Name, response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Hourly forecast request failed for {City}", location.Name);
            return null;
        }
    }

    private async Task<GeocodingResult?> ResolveLocationAsync(string city, CancellationToken cancellationToken)
    {
        var url = string.Format(GeocodingUrl, Uri.EscapeDataString(city));
        logger.LogInformation("Resolving city {City} via Open-Meteo geocoding", city);

        try
        {
            var response = await httpClient.GetFromJsonAsync<GeocodingResponse>(url, cancellationToken);
            var location = response?.Results?.FirstOrDefault();

            if (location is null)
                logger.LogWarning("City {City} was not found in geocoding results", city);
            else
                logger.LogInformation(
                    "Resolved {City} to {Latitude},{Longitude} in {Country}",
                    location.Name, location.Latitude, location.Longitude, location.Country);

            return location;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Geocoding request failed for city {City}", city);
            return null;
        }
    }

    private async Task<ForecastResponse?> FetchForecastAsync(GeocodingResult location, CancellationToken cancellationToken)
    {
        var url = string.Format(CultureInfo.InvariantCulture, ForecastUrl, location.Latitude, location.Longitude);
        logger.LogInformation(
            "Fetching forecast for {City} at {Latitude},{Longitude}",
            location.Name, location.Latitude, location.Longitude);

        try
        {
            var forecast = await httpClient.GetFromJsonAsync<ForecastResponse>(url, cancellationToken);

            if (forecast is null)
                logger.LogWarning("No forecast data returned for {City}", location.Name);

            return forecast;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Forecast request failed for {City} at {Latitude},{Longitude}",
                location.Name, location.Latitude, location.Longitude);
            return null;
        }
    }
}

