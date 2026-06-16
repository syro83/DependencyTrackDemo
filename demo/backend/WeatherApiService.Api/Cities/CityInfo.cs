using System.Text.Json.Serialization;

namespace WeatherApiService.Api.Cities;

record CityInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lon")] double Lon,
    [property: JsonPropertyName("imageUrl")] string ImageUrl = ""
);
