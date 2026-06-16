using System.Reflection;
using System.Text.Json;

namespace WeatherApiService.Api.Cities;

sealed class CitiesRepository
{
    private static readonly string ResourceName =
        $"{typeof(CitiesRepository).Assembly.GetName().Name}.Cities.cities.json";

    public IReadOnlyList<CityInfo> All { get; } = Load();

    private static IReadOnlyList<CityInfo> Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        return JsonSerializer.Deserialize<List<CityInfo>>(stream)
            ?? throw new InvalidOperationException("cities.json deserialized to null.");
    }
}
