using System.Text.Json.Serialization;

namespace WeatherApiService.Api.WeatherCity.OpenMeteo;

record GeocodingResponse(
    [property: JsonPropertyName("results")] IReadOnlyList<GeocodingResult>? Results
);

record GeocodingResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("country_code")] string CountryCode
);

record ForecastResponse(
    [property: JsonPropertyName("daily")] DailyData Daily
);

record HourlyForecastApiResponse(
    [property: JsonPropertyName("hourly")] HourlyData Hourly
);

record HourlyData(
    [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
    [property: JsonPropertyName("temperature_2m")] IReadOnlyList<double> Temperature,
    [property: JsonPropertyName("precipitation_probability")] IReadOnlyList<int?> PrecipitationProbability,
    [property: JsonPropertyName("weathercode")] IReadOnlyList<int> WeatherCode,
    [property: JsonPropertyName("wind_speed_10m")] IReadOnlyList<double> WindSpeed
);

record DailyData(
    [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
    [property: JsonPropertyName("temperature_2m_max")] IReadOnlyList<double> TemperatureMax,
    [property: JsonPropertyName("temperature_2m_min")] IReadOnlyList<double> TemperatureMin,
    [property: JsonPropertyName("weathercode")] IReadOnlyList<int> WeatherCode,
    [property: JsonPropertyName("precipitation_sum")] IReadOnlyList<double> PrecipitationSum,
    [property: JsonPropertyName("wind_speed_10m_max")] IReadOnlyList<double> WindSpeedMax,
    [property: JsonPropertyName("uv_index_max")] IReadOnlyList<double> UvIndexMax,
    [property: JsonPropertyName("sunrise")] IReadOnlyList<string> Sunrise,
    [property: JsonPropertyName("sunset")] IReadOnlyList<string> Sunset,
    [property: JsonPropertyName("sunshine_duration")] IReadOnlyList<double> SunshineDuration,
    [property: JsonPropertyName("precipitation_probability_max")] IReadOnlyList<int?> PrecipitationProbabilityMax
);
