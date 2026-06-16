using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherApiService.Api.Cities;
using WeatherApiService.Api.WeatherCity;
using WeatherApiService.Api.WeatherImages;

namespace WeatherApiService.Tests.WeatherCity;

public sealed class WeatherCityServiceTests
{
    // Realistic 5-day forecast payload as returned by the Open-Meteo API.
    private const string ForecastJson = """
        {
          "daily": {
            "time": ["2026-04-24","2026-04-25","2026-04-26","2026-04-27","2026-04-28"],
            "temperature_2m_max": [20.0, 22.0, 18.0, 15.0, 25.0],
            "temperature_2m_min": [10.0, 12.0,  8.0,  5.0, 15.0],
            "weathercode": [0, 1, 2, 3, 61],
            "precipitation_sum": [0.0, 1.2, 3.5, 0.0, 5.0],
            "wind_speed_10m_max": [12.0, 18.0, 22.0, 9.0, 30.0],
            "uv_index_max": [3.5, 4.1, 2.0, 5.0, 6.2],
            "sunshine_duration": [21600.0, 18000.0, 10800.0, 25200.0, 28800.0],
            "precipitation_probability_max": [20, 40, 70, 10, 5],
            "sunrise": ["2026-04-24T06:10","2026-04-25T06:09","2026-04-26T06:07","2026-04-27T06:06","2026-04-28T06:04"],
            "sunset":  ["2026-04-24T20:43","2026-04-25T20:44","2026-04-26T20:46","2026-04-27T20:47","2026-04-28T20:49"]
          }
        }
        """;

    private const string GeocodingJson = """
        {
          "results": [
            {
              "name": "UnknownCity",
              "latitude": 50.0,
              "longitude": 10.0,
              "country": "Testland",
              "country_code": "TL"
            }
          ]
        }
        """;

    // ── known city (skips geocoding) ─────────────────────────────────────────

    [Fact]
    public async Task GetForecastAsync_KnownCity_ReturnsForecastWithoutGeocodingCall()
    {
        var (service, handler) = BuildService(url =>
        {
            // Geocoding must NOT be called — if it is, return 500 to fail the test.
            if (url.Contains("geocoding"))
                return Response(HttpStatusCode.InternalServerError, "{}");

            return Response(HttpStatusCode.OK, ForecastJson);
        });

        var firstCity = new CitiesRepository().All[0];
        var result    = await service.GetForecastAsync(firstCity.Name);

        Assert.NotNull(result);
        Assert.Equal(firstCity.Name,    result.City);
        Assert.Equal(firstCity.Country, result.Country);
        Assert.Equal(5, result.Forecast.Count);
    }

    [Fact]
    public async Task GetForecastAsync_KnownCity_ForecastHttpFails_ReturnsNull()
    {
        var (service, _) = BuildService(_ => Response(HttpStatusCode.ServiceUnavailable, "{}"));
        var firstCity = new CitiesRepository().All[0];

        var result = await service.GetForecastAsync(firstCity.Name);

        Assert.Null(result);
    }

    // ── unknown city (goes through geocoding) ────────────────────────────────

    [Fact]
    public async Task GetForecastAsync_UnknownCity_ResolvesViaGeocodingThenReturnsForecast()
    {
        var (service, _) = BuildService(url =>
        {
            if (url.Contains("geocoding"))
                return Response(HttpStatusCode.OK, GeocodingJson);

            return Response(HttpStatusCode.OK, ForecastJson);
        });

        var result = await service.GetForecastAsync("UnknownCity__9999");

        Assert.NotNull(result);
        Assert.Equal("UnknownCity", result.City);
        Assert.Equal(5, result.Forecast.Count);
    }

    [Fact]
    public async Task GetForecastAsync_UnknownCity_GeocodingReturnsEmpty_ReturnsNull()
    {
        var (service, _) = BuildService(_ => Response(HttpStatusCode.OK, """{"results":[]}"""));

        var result = await service.GetForecastAsync("NowhereAtAll__9999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetForecastAsync_UnknownCity_GeocodingHttpFails_ReturnsNull()
    {
        var (service, _) = BuildService(_ => Response(HttpStatusCode.BadGateway, "{}"));

        var result = await service.GetForecastAsync("NowhereAtAll__9999");

        Assert.Null(result);
    }

    // ── random city ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRandomCityForecastAsync_ReturnsForecastForSomeCity()
    {
        var (service, _) = BuildService(_ => Response(HttpStatusCode.OK, ForecastJson));

        var result = await service.GetRandomCityForecastAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.City);
        Assert.Equal(5, result.Forecast.Count);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static (WeatherCityService service, FakeHttpMessageHandler handler) BuildService(
        Func<string, HttpResponseMessage> respond)
    {
        var handler    = new FakeHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler);
        var service    = new WeatherCityService(
            httpClient,
            new CitiesRepository(),
            new WeatherImagesRepository(),
            NullLogger<WeatherCityService>.Instance);

        return (service, handler);
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string json) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}

sealed class FakeHttpMessageHandler(Func<string, HttpResponseMessage> respond) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(respond(request.RequestUri!.ToString()));
}
