namespace WeatherApiService.Api.WeatherCity;

record CityForecastResponse(
    string City,
    string Country,
    string Region,
    double Latitude,
    double Longitude,
    string CityImageUrl,
    IReadOnlyList<CityDayForecast> Forecast
);

record CityDayForecast(
    DateOnly Date,
    int TemperatureMaxC,
    int TemperatureMinC,
    int TemperatureMaxF,
    int TemperatureMinF,
    string Summary,
    string WeatherImageUrl,
    double PrecipitationMm,
    int WindSpeedKmh,
    double UvIndex,
    string Sunrise,
    string Sunset,
    double SunshineHours,
    int? PrecipitationProbability
);

record CityHourlyForecastResponse(
    string City,
    IReadOnlyList<CityHourForecast> Hours
);

record CityHourForecast(
    string Time,
    int TemperatureC,
    int? PrecipitationProbability,
    string Summary,
    int WindSpeedKmh
);

