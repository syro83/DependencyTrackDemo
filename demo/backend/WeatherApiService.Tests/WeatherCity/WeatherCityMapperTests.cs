using WeatherApiService.Api.WeatherCity;
using WeatherApiService.Api.WeatherCity.OpenMeteo;
using WeatherApiService.Api.WeatherImages;

namespace WeatherApiService.Tests.WeatherCity;

public sealed class WeatherCityMapperTests
{
    // ── ToFahrenheit (tested indirectly via ToResponse) ──────────────────────

    [Theory]
    [InlineData(0,    0,    32,  32)]   // 0 °C = 32 °F
    [InlineData(100,  80,   212, 176)]  // 100 °C = 212 °F, 80 °C = 176 °F
    [InlineData(-40, -40,  -40, -40)]   // -40 °C = -40 °F (crossover)
    [InlineData(20,   10,   68,  50)]   // typical values
    public void ToResponse_ConvertsTemperaturesCorrectly(
        int maxC, int minC, int expectedMaxF, int expectedMinF)
    {
        var location = new GeocodingResult("TestCity", 52.0, 4.0, "TestCountry", "TC");
        var forecast  = BuildForecast([(maxC, minC, 0)]);
        var images    = new WeatherImagesRepository();

        var response = WeatherCityMapper.ToResponse(location, forecast, "TestRegion", "http://img", images);

        var day = response.Forecast[0];
        Assert.Equal(maxC, day.TemperatureMaxC);
        Assert.Equal(minC, day.TemperatureMinC);
        Assert.Equal(expectedMaxF, day.TemperatureMaxF);
        Assert.Equal(expectedMinF, day.TemperatureMinF);
    }

    // ── WMO code → summary mapping ───────────────────────────────────────────

    [Theory]
    [InlineData(0,  "Clear Sky")]
    [InlineData(1,  "Mainly Clear")]
    [InlineData(2,  "Partly Cloudy")]
    [InlineData(3,  "Overcast")]
    [InlineData(45, "Foggy")]
    [InlineData(48, "Foggy")]
    [InlineData(51, "Light Drizzle")]
    [InlineData(53, "Light Drizzle")]
    [InlineData(55, "Dense Drizzle")]
    [InlineData(56, "Freezing Drizzle")]
    [InlineData(57, "Freezing Drizzle")]
    [InlineData(61, "Rain")]
    [InlineData(63, "Rain")]
    [InlineData(65, "Heavy Rain")]
    [InlineData(66, "Freezing Rain")]
    [InlineData(67, "Freezing Rain")]
    [InlineData(71, "Snow")]
    [InlineData(73, "Snow")]
    [InlineData(75, "Heavy Snow")]
    [InlineData(77, "Snow Grains")]
    [InlineData(80, "Rain Showers")]
    [InlineData(81, "Rain Showers")]
    [InlineData(82, "Violent Rain Showers")]
    [InlineData(85, "Snow Showers")]
    [InlineData(86, "Snow Showers")]
    [InlineData(95, "Thunderstorm")]
    [InlineData(96, "Thunderstorm with Hail")]
    [InlineData(99, "Thunderstorm with Hail")]
    [InlineData(999, "Unknown")]
    public void ToResponse_MapsWmoCodeToExpectedSummary(int wmoCode, string expectedSummary)
    {
        var location = new GeocodingResult("TestCity", 52.0, 4.0, "TestCountry", "TC");
        var forecast  = BuildForecast([(20, 10, wmoCode)]);
        var images    = new WeatherImagesRepository();

        var response = WeatherCityMapper.ToResponse(location, forecast, "Region", "http://img", images);

        Assert.Equal(expectedSummary, response.Forecast[0].Summary);
    }

    // ── ToResponse — general shape ────────────────────────────────────────────

    [Fact]
    public void ToResponse_MapsLocationFieldsCorrectly()
    {
        var location = new GeocodingResult("Amsterdam", 52.37, 4.89, "Netherlands", "NL");
        var forecast  = BuildForecast([(15, 8, 2)]);
        var images    = new WeatherImagesRepository();

        var response = WeatherCityMapper.ToResponse(location, forecast, "Noord-Holland", "http://img/amsterdam", images);

        Assert.Equal("Amsterdam",    response.City);
        Assert.Equal("Netherlands",  response.Country);
        Assert.Equal("Noord-Holland",response.Region);
        Assert.Equal(52.37,          response.Latitude);
        Assert.Equal(4.89,           response.Longitude);
        Assert.Equal("http://img/amsterdam", response.CityImageUrl);
    }

    [Fact]
    public void ToResponse_ProducesDayForecastForEachTimeEntry()
    {
        var location = new GeocodingResult("TestCity", 0.0, 0.0, "TC", "TC");
        var forecast  = BuildForecast([(20, 10, 0), (22, 12, 1), (18, 8, 3)]);
        var images    = new WeatherImagesRepository();

        var response = WeatherCityMapper.ToResponse(location, forecast, "", "", images);

        Assert.Equal(3, response.Forecast.Count);
        Assert.Equal(new DateOnly(2026, 4, 24), response.Forecast[0].Date);
        Assert.Equal(new DateOnly(2026, 4, 25), response.Forecast[1].Date);
        Assert.Equal(new DateOnly(2026, 4, 26), response.Forecast[2].Date);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static ForecastResponse BuildForecast(IList<(int maxC, int minC, int code)> days)
    {
        var dates    = Enumerable.Range(0, days.Count)
                           .Select(i => DateOnly.FromDateTime(new DateTime(2026, 4, 24)).AddDays(i).ToString("yyyy-MM-dd"))
                           .ToList();
        var maxTemps  = days.Select(d => (double)d.maxC).ToList();
        var minTemps  = days.Select(d => (double)d.minC).ToList();
        var codes     = days.Select(d => d.code).ToList();
        var precip    = Enumerable.Repeat(2.5, days.Count).ToList();
        var wind      = Enumerable.Repeat(15.0, days.Count).ToList();
        var uv        = Enumerable.Repeat(4.2, days.Count).ToList();
        var sunrises  = Enumerable.Range(0, days.Count)
                           .Select(i => $"{DateOnly.FromDateTime(new DateTime(2026, 4, 24)).AddDays(i):yyyy-MM-dd}T06:12")
                           .ToList();
        var sunsets   = Enumerable.Range(0, days.Count)
                           .Select(i => $"{DateOnly.FromDateTime(new DateTime(2026, 4, 24)).AddDays(i):yyyy-MM-dd}T20:45")
                           .ToList();
        var sunshine  = Enumerable.Repeat(21600.0, days.Count).ToList(); // 6 hours in seconds
        var precipProb = Enumerable.Repeat<int?>(40, days.Count).ToList();

        return new ForecastResponse(new DailyData(dates, maxTemps, minTemps, codes, precip, wind, uv, sunrises, sunsets, sunshine, precipProb));
    }
}
