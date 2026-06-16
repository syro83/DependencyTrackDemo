namespace DependencyTrackHelper.Api.Models;

internal sealed class ProjectProcessRequest
{
    public string ProjectName { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public bool? ActivateVersion { get; init; }

    public int? CleanInactiveProjects { get; init; }

    public string? ParentName { get; init; }
}
