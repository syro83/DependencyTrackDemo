using System.Reflection;
using System.Text.Json;

namespace WeatherApiService.Api.WeatherImages;

sealed class WeatherImagesRepository
{
    private static readonly string ResourceName =
        $"{typeof(WeatherImagesRepository).Assembly.GetName().Name}.WeatherImages.weather-images.json";

    private readonly IReadOnlyDictionary<string, string[]> _imagesBySummary;

    public WeatherImagesRepository()
    {
        _imagesBySummary = Load();
    }

    public string PickImage(string summary)
    {
        if (_imagesBySummary.TryGetValue(summary, out var images) && images.Length > 0)
            return images[Random.Shared.Next(images.Length)];

        if (_imagesBySummary.TryGetValue("Unknown", out var fallback) && fallback.Length > 0)
            return fallback[Random.Shared.Next(fallback.Length)];

        return string.Empty;
    }

    private static IReadOnlyDictionary<string, string[]> Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        return JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream)
            ?? throw new InvalidOperationException("weather-images.json deserialized to null.");
    }
}
