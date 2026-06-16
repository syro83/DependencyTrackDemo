using WeatherApiService.Api.WeatherImages;

namespace WeatherApiService.Tests.WeatherImages;

public sealed class WeatherImagesRepositoryTests
{
    private readonly WeatherImagesRepository _repo = new();

    [Theory]
    [InlineData("Clear Sky")]
    [InlineData("Rain")]
    [InlineData("Snow")]
    [InlineData("Thunderstorm")]
    [InlineData("Overcast")]
    public void PickImage_KnownSummary_ReturnsNonEmptyUrl(string summary)
    {
        var url = _repo.PickImage(summary);
        Assert.False(string.IsNullOrWhiteSpace(url), $"Expected a non-empty URL for summary '{summary}'.");
    }

    [Fact]
    public void PickImage_UnknownSummary_ReturnsFallbackOrEmpty()
    {
        // Should not throw — must return either the "Unknown" fallback or empty string.
        var url = _repo.PickImage("SomethingCompletelyMadeUp");
        Assert.NotNull(url);
    }

    [Fact]
    public void PickImage_CalledMultipleTimes_ReturnsConsistentlyNonEmpty()
    {
        var urls = Enumerable.Range(0, 20)
            .Select(_ => _repo.PickImage("Clear Sky"))
            .ToList();

        Assert.All(urls, url => Assert.False(string.IsNullOrWhiteSpace(url)));
    }
}
