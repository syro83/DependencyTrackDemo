using System.Text;
using System.Text.Json;

using DependencyTrackHelper.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DependencyTrackHelper.Api.Clients;

internal sealed class DependencyTrackClient(
    IHttpClientFactory httpClientFactory,
    IOptions<DependencyTrackOptions> dependencyTrackOptions,
    ILogger<DependencyTrackClient> logger) : IDependencyTrackClient
{
    public Task<ClientResult<List<DependencyTrackProjectDto>>> GetProjectsAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken) =>
        SendForObjectAsync<List<DependencyTrackProjectDto>>(processContext, HttpMethod.Get, "/api/v1/project", payload: null, cancellationToken);

    public Task<ClientResult<List<DependencyTrackProjectDto>>> GetAllProjectsAsync(
        ProjectProcessContext processContext,
        bool excludeInactive = false,
        bool onlyRoot = false,
        string? searchText = "",
        int pageSize = 100,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var relativePath =
            $"/api/v1/project?excludeInactive={(excludeInactive ? "true" : "false")}" +
            $"&onlyRoot={(onlyRoot ? "true" : "false")}" +
            $"&searchText={Uri.EscapeDataString(searchText ?? string.Empty)}" +
            $"&pageSize={pageSize}" +
            $"&pageNumber={pageNumber}";

        return SendForObjectAsync<List<DependencyTrackProjectDto>>(
            processContext,
            HttpMethod.Get,
            relativePath,
            payload: null,
            cancellationToken);
    }

    public Task<ClientResult<DependencyTrackProjectDto>> LookupProjectAsync(
        ProjectProcessContext processContext,
        string name,
        string? version,
        CancellationToken cancellationToken)
    {
        var relativePath = $"/api/v1/project/lookup?name={Uri.EscapeDataString(name)}";
        if (!string.IsNullOrWhiteSpace(version))
        {
            relativePath += $"&version={Uri.EscapeDataString(version)}";
        }

        return SendForObjectAsync<DependencyTrackProjectDto>(processContext, HttpMethod.Get, relativePath, payload: null, cancellationToken);
    }

    public Task<ClientResult<DependencyTrackProjectDto>> CreateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackCreateProjectRequest request,
        CancellationToken cancellationToken) =>
        SendForObjectAsync<DependencyTrackProjectDto>(processContext, HttpMethod.Put, "/api/v1/project", request, cancellationToken);

    public Task<ClientResult<DependencyTrackProjectDto>> UpdateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackUpdateProjectRequest request,
        CancellationToken cancellationToken) =>
        SendForObjectAsync<DependencyTrackProjectDto>(processContext, HttpMethod.Post, "/api/v1/project", request, cancellationToken);

    public Task<ClientResult<object>> DeleteProjectAsync(
        ProjectProcessContext processContext,
        string projectUuid,
        CancellationToken cancellationToken) =>
        SendForObjectAsync<object>(processContext, HttpMethod.Delete, $"/api/v1/project/{projectUuid}", payload: null, cancellationToken);

    // Centralized HTTP execution path: resolves URL, applies request context headers,
    // serializes payload, evaluates status code, and returns a typed repository result.
    private async Task<ClientResult<T>> SendForObjectAsync<T>(
        ProjectProcessContext processContext,
        HttpMethod method,
        string relativePath,
        object? payload,
        CancellationToken cancellationToken)
    {
        var precheck = GetBaseUrlPrecheck();
        if (!precheck.IsSuccess)
        {
            return new ClientResult<T>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Problem = precheck.Problem,
                RawBody = precheck.Problem is null ? null : JsonSerializer.Serialize(precheck.Problem)
            };
        }

        var targetUrl = ResolveTargetUrl(precheck.BaseUrl!, relativePath);

        var apiKey = processContext.ApiKey;

        using var request = new HttpRequestMessage(method, targetUrl);

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Remove("X-Api-Key");
            request.Headers.Add("X-Api-Key", apiKey);
        }

        if (payload is not null)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            logger.LogInformation(
                "Sending Dependency-Track request. Method={Method}, TargetUrl={TargetUrl}",
                method,
                targetUrl);

            using var client = httpClientFactory.CreateClient();
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation(
                "Dependency-Track request completed. Method={Method}, StatusCode={StatusCode}",
                method,
                (int)response.StatusCode);

            if (response.StatusCode is < System.Net.HttpStatusCode.OK or >= System.Net.HttpStatusCode.MultipleChoices)
            {
                logger.LogWarning(
                    "Dependency-Track request returned a non-success response. Method={Method}, TargetUrl={TargetUrl}, StatusCode={StatusCode}, Body={Body}",
                    method,
                    targetUrl,
                    (int)response.StatusCode,
                    body);

                return new ClientResult<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Problem = TryDeserializeProblem(body),
                    RawBody = body
                };
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return new ClientResult<T>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = default,
                    RawBody = body
                };
            }

            try
            {
                var data = JsonSerializer.Deserialize<T>(body, JsonOptions());
                return new ClientResult<T>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = data,
                    RawBody = body
                };
            }
            catch (JsonException)
            {
                // Successful status with invalid JSON is treated as an upstream contract issue.
                return new ClientResult<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Problem = new ProblemDetails
                    {
                        Status = (int)response.StatusCode,
                        Title = "Invalid response",
                        Detail = "Dependency-Track returned a response body that could not be parsed."
                    },
                    RawBody = body
                };
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Dependency-Track request failed. Method={Method}, TargetUrl={TargetUrl}",
                method,
                targetUrl);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Dependency-Track request failed",
                Detail = ex.Message
            };

            return new ClientResult<T>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status502BadGateway,
                Problem = problem,
                RawBody = JsonSerializer.Serialize(problem)
            };
        }
    }

    // Reads and validates Dependency-Track base URL from options.
    private BaseUrlPrecheckResult GetBaseUrlPrecheck()
    {
        var baseUrl = dependencyTrackOptions.Value.BaseUrl?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return new BaseUrlPrecheckResult(baseUrl, null, true);
        }

        return new BaseUrlPrecheckResult(
            null,
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Dependency-Track proxy is not configured",
                Detail = "Set DependencyTrack:BaseUrl."
            },
            false);
    }

    // Supports both absolute URLs and repository-relative API paths.
    private static string ResolveTargetUrl(string baseUrl, string relativePath)
    {
        if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return relativePath;
        }

        return $"{baseUrl}/{relativePath.TrimStart('/')}";
    }

    // Best-effort conversion of upstream error payloads to ProblemDetails.
    private static ProblemDetails? TryDeserializeProblem(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ProblemDetails>(body, JsonOptions());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record BaseUrlPrecheckResult(string? BaseUrl, ProblemDetails? Problem, bool IsSuccess);
}
