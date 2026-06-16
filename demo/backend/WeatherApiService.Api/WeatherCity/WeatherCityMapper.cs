using WeatherApiService.Api.WeatherCity.OpenMeteo;
using WeatherApiService.Api.WeatherImages;

namespace WeatherApiService.Api.WeatherCity;

static class WeatherCityMapper
{
    public static CityHourlyForecastResponse ToHourlyResponse(string cityName, HourlyForecastApiResponse response)
    {
        var hourly = response.Hourly;
        var hours = hourly.Time
            .Select((timeString, i) =>
            {
                var summary = WmoCodeToSummary(hourly.WeatherCode[i]);
                // time is "2026-04-24T14:00" — strip the date part
                var t = timeString.IndexOf('T');
                var label = t >= 0 ? timeString[(t + 1)..] : timeString;
                return new CityHourForecast(
                    Time: label,
                    TemperatureC: (int)Math.Round(hourly.Temperature[i]),
                    PrecipitationProbability: hourly.PrecipitationProbability[i],
                    Summary: summary,
                    WindSpeedKmh: (int)Math.Round(hourly.WindSpeed[i])
                );
            })
            .ToList();

        return new CityHourlyForecastResponse(City: cityName, Hours: hours);
    }

    public static CityForecastResponse ToResponse(
        GeocodingResult location,
        ForecastResponse forecast,
        string region,
        string cityImageUrl,
        WeatherImagesRepository imagesRepository)
    {
        var daily = forecast.Daily;
        var days = daily.Time
            .Select((dateString, i) =>
            {
                var summary = WmoCodeToSummary(daily.WeatherCode[i]);
                return new CityDayForecast(
                    Date: DateOnly.Parse(dateString),
                    TemperatureMaxC: Round(daily.TemperatureMax[i]),
                    TemperatureMinC: Round(daily.TemperatureMin[i]),
                    TemperatureMaxF: ToFahrenheit(Round(daily.TemperatureMax[i])),
                    TemperatureMinF: ToFahrenheit(Round(daily.TemperatureMin[i])),
                    Summary: summary,
                    WeatherImageUrl: imagesRepository.PickImage(summary),
                    PrecipitationMm: Math.Round(daily.PrecipitationSum[i], 1),
                    WindSpeedKmh: Round(daily.WindSpeedMax[i]),
                    UvIndex: Math.Round(daily.UvIndexMax[i], 1),
                    Sunrise: FormatTime(daily.Sunrise[i]),
                    Sunset: FormatTime(daily.Sunset[i]),
                    SunshineHours: Math.Round(daily.SunshineDuration[i] / 3600.0, 1),
                    PrecipitationProbability: daily.PrecipitationProbabilityMax[i]
                );
            })
            .ToList();

        return new CityForecastResponse(
            City: location.Name,
            Country: location.Country,
            Region: region,
            Latitude: location.Latitude,
            Longitude: location.Longitude,
            CityImageUrl: cityImageUrl,
            Forecast: days
        );
    }

    private static int Round(double value) => (int)Math.Round(value);

    private static int ToFahrenheit(int celsius) => celsius * 9 / 5 + 32;

    // Open-Meteo returns sunrise/sunset as "2026-04-24T06:12" — strip the date part.
    private static string FormatTime(string isoDateTime)
    {
        var t = isoDateTime.IndexOf('T');
        return t >= 0 ? isoDateTime[(t + 1)..] : isoDateTime;
    }

    // WMO Weather Interpretation Codes → human-readable summary.
    // Reference: https://open-meteo.com/en/docs#weathervariables
    private static string WmoCodeToSummary(int code) => code switch
    {
        0          => "Clear Sky",
        1          => "Mainly Clear",
        2          => "Partly Cloudy",
        3          => "Overcast",
        45 or 48   => "Foggy",
        51 or 53   => "Light Drizzle",
        55         => "Dense Drizzle",
        56 or 57   => "Freezing Drizzle",
        61 or 63   => "Rain",
        65         => "Heavy Rain",
        66 or 67   => "Freezing Rain",
        71 or 73   => "Snow",
        75         => "Heavy Snow",
        77         => "Snow Grains",
        80 or 81   => "Rain Showers",
        82         => "Violent Rain Showers",
        85 or 86   => "Snow Showers",
        95         => "Thunderstorm",
        96 or 99   => "Thunderstorm with Hail",
        _          => "Unknown"
    };
}

