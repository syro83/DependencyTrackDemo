using DependencyTrackHelper.Api.Models;

namespace DependencyTrackHelper.Api.Services;

internal interface IProjectProcessService
{
    Task<IResult> ProcessAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken);
}
