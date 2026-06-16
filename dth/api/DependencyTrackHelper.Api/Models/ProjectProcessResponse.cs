namespace DependencyTrackHelper.Api.Models;

internal sealed class ProjectProcessResponse
{
    public string ProjectUuid { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string? Version { get; init; }

    public bool? Active { get; init; }

    public bool? IsLatest { get; init; }

    public string? ParentUuid { get; init; }

    public int? DeactivatedCount { get; init; }

    public int? DeletedInactiveCount { get; init; }
}
