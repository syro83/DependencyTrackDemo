namespace DependencyTrackHelper.Api.Models;

internal sealed class ProjectProcessContext
{
    public string ApiKey { get; init; } = string.Empty;

    public ProjectProcessRequest Request { get; init; } = new();

    public DependencyTrackProjectDto? TargetProject { get; set; }

    public DependencyTrackProjectDto? ParentProject { get; set; }
}
