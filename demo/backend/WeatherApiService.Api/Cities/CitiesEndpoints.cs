namespace WeatherApiService.Api.Cities;

static class CitiesEndpoints
{
    public static IEndpointRouteBuilder MapCitiesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/cities", (CitiesRepository repository) => Results.Ok(repository.All))
           .WithName("GetCities");

        return app;
    }
}
