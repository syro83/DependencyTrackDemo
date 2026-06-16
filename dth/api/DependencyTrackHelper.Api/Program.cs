using DependencyTrackHelper.Api.Models;
using DependencyTrackHelper.Api.Clients;
using DependencyTrackHelper.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile($"appsettings.json")
    .AddEnvironmentVariables();

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromSeconds(63_072_000); // 2 dagen
    options.IncludeSubDomains = true;
    options.Preload = true;
});

builder.Services.AddHttpClient();
builder.Services.Configure<DependencyTrackOptions>(
    builder.Configuration.GetSection(DependencyTrackOptions.SectionName));
builder.Services.AddScoped<IDependencyTrackClient, DependencyTrackClient>();
builder.Services.AddScoped<IProjectProcessService, ProjectProcessService>();

var app = builder.Build();

app.Use((context, next) =>
{
    context.Request.Scheme = Uri.UriSchemeHttps;
    return next(context);
});

app.UseHsts();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck")
    .Produces(StatusCodes.Status200OK);

app.MapPost(
    "/api/v1/projectprocess",
    async (
        HttpContext context,
        ProjectProcessRequest request,
        IProjectProcessService service,
        CancellationToken cancellationToken) =>
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) ||
            string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            return Results.Problem(
                title: "Missing API key",
                detail: "Provide header X-Api-Key.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(request.ProjectName) ||
            string.IsNullOrWhiteSpace(request.Version))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "projectName and version are required in the request body.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var processContext = new ProjectProcessContext
        {
            ApiKey = apiKeyHeader.ToString(),
            Request = request
        };

        return await service.ProcessAsync(processContext, cancellationToken);
    })
    .WithName("ProjectProcess")
    .WithSummary("Process a Dependency-Track project lifecycle.")
    .WithDescription("Ensures parent exists, activates the target version, deactivates siblings, and cleans up old inactive projects.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .ProducesProblem(StatusCodes.Status502BadGateway);

app.Run();
