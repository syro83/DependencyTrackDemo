using Azure.Storage.Blobs;
using Azure.Identity;
using WeatherApiService.Api.Cities;
using WeatherApiService.Api.WeatherCity;
using WeatherApiService.Api.WeatherImages;

var builder = WebApplication.CreateBuilder(args);

var appConfigBlobUrl = builder.Configuration.GetValue<string>("Endpoints:AppConfigBlob");

if (!string.IsNullOrWhiteSpace(appConfigBlobUrl))
{
    var blobClient = new BlobClient(new Uri(appConfigBlobUrl), new DefaultAzureCredential());
    var download = blobClient.DownloadStreaming();
    builder.Configuration.AddJsonStream(download.Value.Content);
}
else if (!builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("Endpoints:AppConfigBlob must be configured outside development.");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside development.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("WeatherClient", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Application Insights telemetry — picks up APPLICATIONINSIGHTS_CONNECTION_STRING automatically.
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddHealthChecks();

// Typed HttpClient for the real-weather city forecast feature (Open-Meteo, no API key required).
builder.Services.AddHttpClient<IWeatherCityService, WeatherCityService>();

// Static city list loaded once from the embedded cities.json resource.
builder.Services.AddSingleton<CitiesRepository>();

// Weather images loaded once from the embedded weather-images.json resource.
builder.Services.AddSingleton<WeatherImagesRepository>();

// Stream logs to App Service file system and blob storage when running on Azure.
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddAzureWebAppDiagnostics();
}

var app = builder.Build();

var startupLogger = app.Logger;

startupLogger.LogInformation(
    "WeatherApiService starting in {Environment} environment",
    app.Environment.EnvironmentName);

if (!string.IsNullOrWhiteSpace(appConfigBlobUrl))
    startupLogger.LogInformation("App config blob URL: {Url}", appConfigBlobUrl);
else
    startupLogger.LogWarning("App config blob URL is not configured — using local settings only");

startupLogger.LogInformation(
    "CORS policy configured with {OriginCount} allowed origin(s)",
    allowedOrigins.Length);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("WeatherClient");

app.MapHealthChecks("/healthz");
app.MapCitiesEndpoints();
app.MapWeatherEndpoints();

FluentAssertions.AssertionEngine.Configuration.Formatting.MaxDepth = 100;

app.Run();

