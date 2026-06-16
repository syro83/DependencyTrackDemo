using DependencyTrackHelper.Api.Models;

namespace DependencyTrackHelper.Api.Clients;

internal interface IDependencyTrackClient
{
    Task<ClientResult<List<DependencyTrackProjectDto>>> GetProjectsAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken);

    Task<ClientResult<List<DependencyTrackProjectDto>>> GetAllProjectsAsync(
        ProjectProcessContext processContext,
        bool excludeInactive = false,
        bool onlyRoot = false,
        string? searchText = "",
        int pageSize = 100,
        int pageNumber = 1,
        CancellationToken cancellationToken = default);

    Task<ClientResult<DependencyTrackProjectDto>> LookupProjectAsync(
        ProjectProcessContext processContext,
        string name,
        string? version,
        CancellationToken cancellationToken);

    Task<ClientResult<DependencyTrackProjectDto>> CreateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackCreateProjectRequest request,
        CancellationToken cancellationToken);

    Task<ClientResult<DependencyTrackProjectDto>> UpdateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackUpdateProjectRequest request,
        CancellationToken cancellationToken);

    Task<ClientResult<object>> DeleteProjectAsync(
        ProjectProcessContext processContext,
        string projectUuid,
        CancellationToken cancellationToken);
}
