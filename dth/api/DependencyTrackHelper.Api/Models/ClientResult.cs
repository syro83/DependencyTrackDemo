using Microsoft.AspNetCore.Mvc;

namespace DependencyTrackHelper.Api.Models;

internal sealed class ClientResult<T>
{
    public bool IsSuccess { get; init; }

    public int StatusCode { get; init; }

    public T? Data { get; init; }

    public ProblemDetails? Problem { get; init; }

    public string? RawBody { get; init; }
}
