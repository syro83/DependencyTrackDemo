using DependencyTrackHelper.Api.Models;
using DependencyTrackHelper.Api.Clients;

namespace DependencyTrackHelper.Api.Services;

internal sealed class ProjectProcessService(
    IDependencyTrackClient dependencyTrackClient,
    ILogger<ProjectProcessService> logger) : IProjectProcessService
{
    /// <summary>
    /// Runs project processing in four phases:
    /// 1) Ensure parent: if parentName is provided, look it up and create it when missing.
    /// 2) Activate target: resolve the requested project by name/version, assign parent, set active, and mark it as latest.
    /// 3) Deactivate siblings: for projects with the same name, set all non-target versions to inactive and not latest.
    /// 4) Cleanup inactive: keep only the configured number of inactive versions and delete older ones.
    /// </summary>
    public async Task<IResult> ProcessAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        using var logScope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["ProjectName"] = processContext.Request.ProjectName,
            ["Version"] = processContext.Request.Version,
        });

        logger.LogInformation(
            "Starting project process. ActivateVersion={ActivateVersion}, CleanInactiveProject={CleanInactiveProject}",
            processContext.Request.ActivateVersion,
            processContext.Request.CleanInactiveProjects);

        // Phase 1
        var parentResultError = await EnsureParentProjectAsync(processContext, cancellationToken);
        if (parentResultError is not null)
        {
            return parentResultError;
        }

        // Phase 2
        var activationResultError = await ActivateTargetProjectAsync(processContext, cancellationToken);
        if (activationResultError is not null)
        {
            return activationResultError;
        }

        // Phase 3
        var deactivatedCount = await DeactivateSiblingProjectsAsync(processContext, cancellationToken);

        // Phase 4
        var deletedInactiveCount = await CleanupInactiveProjectsAsync(processContext, cancellationToken);

        logger.LogInformation(
            "Project process completed. TargetProjectUuid={ProjectUuid}, DeactivatedCount={DeactivatedCount}, DeletedInactiveCount={DeletedInactiveCount}",
            processContext.TargetProject?.Uuid,
            deactivatedCount,
            deletedInactiveCount);

        return Results.Ok(new ProjectProcessResponse
        {
            ProjectUuid = processContext.TargetProject?.Uuid ?? string.Empty,
            ProjectName = processContext.TargetProject?.Name ?? processContext.Request.ProjectName,
            Version = processContext.TargetProject?.Version,
            Active = processContext.TargetProject?.Active,
            IsLatest = processContext.TargetProject?.IsLatest,
            ParentUuid = processContext.ParentProject?.Uuid,
            DeactivatedCount = deactivatedCount,
            DeletedInactiveCount = deletedInactiveCount
        });
    }

    // Phase 1: ensure optional parent exists and return it for later parent assignment.
    private async Task<IResult?> EnsureParentProjectAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Phase 1 started: ensuring parent project.");

        DependencyTrackProjectDto? parentProject = null;
        if (!string.IsNullOrWhiteSpace(processContext.Request.ParentName))
        {
            parentProject = await LookupProjectAsync(processContext, processContext.Request.ParentName, null, cancellationToken);
            if (parentProject is null)
            {
                var parentRequest = new DependencyTrackCreateProjectRequest
                {
                    Name = processContext.Request.ParentName,
                    Active = true,
                    IsLatest = null,
                    Parent = null,
                    Classifier = "APPLICATION",
                    CollectionLogic = "AGGREGATE_LATEST_VERSION_CHILDREN",
                    Description = $"Auto-created parent project for '{processContext.Request.ProjectName}'",
                    Version = null
                };

                parentProject = await CreateProjectAsync(
                    processContext,
                    parentRequest,
                    cancellationToken);

                if (parentProject is null)
                {
                    logger.LogError("Phase 1 failed: could not create parent project {ParentName}.", processContext.Request.ParentName);
                    return Results.Problem(
                        title: "Project processing failed",
                        detail: $"Could not create parent project '{processContext.Request.ParentName}'.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                logger.LogInformation("Parent project created. ParentUuid={ParentUuid}", parentProject.Uuid);
            }
            else
            {
                logger.LogInformation("Parent project found. ParentUuid={ParentUuid}", parentProject.Uuid);
            }

            processContext.ParentProject = parentProject;
        }

        logger.LogInformation("Phase 1 completed.");
        return null;
    }

    // Phase 2: resolve the target version and mark it active/latest under the resolved parent.
    private async Task<IResult?> ActivateTargetProjectAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Phase 2 started: resolving and updating target project.");

        var targetProject = await LookupProjectAsync(processContext, processContext.Request.ProjectName, processContext.Request.Version, cancellationToken);
        if (targetProject is null)
        {
            logger.LogWarning("Phase 2 failed: project not found.");
            return Results.Problem(
                title: "Project not found",
                detail: $"Project '{processContext.Request.ProjectName}' with version '{processContext.Request.Version}' does not exist.",
                statusCode: StatusCodes.Status404NotFound);
        }


        // Update the target project: assign parent, set active, and mark as latest.
        targetProject.Active = true;
        targetProject.IsLatest = true;

        var parentUuid = processContext.ParentProject?.Uuid;
        if (!string.IsNullOrWhiteSpace(parentUuid))
        {
            targetProject.Parent = new DependencyTrackProjectReferenceDto { Uuid = parentUuid };
        }


        var updatedTarget = await UpdateProjectAsync(
            processContext,
            targetProject,
            cancellationToken);

        if (updatedTarget is null)
        {
            logger.LogError("Phase 2 failed: update request for target project was unsuccessful.");
            return Results.Problem(
                title: "Project processing failed",
                detail: "Could not update the target project in Dependency-Track.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        processContext.TargetProject = updatedTarget;

        logger.LogInformation("Phase 2 completed. TargetProjectUuid={ProjectUuid}", updatedTarget.Uuid);
        return null;
    }

    // Phase 3: for same-name versions, keep the target as active/latest and deactivate others.
    private async Task<int> DeactivateSiblingProjectsAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Phase 3 started: deactivating sibling active projects.");

        var allProjects = await GetProjectsAsync(processContext.Request.ParentName, processContext, cancellationToken);
        var sameNameProjects = allProjects
            .Where(project => string.Equals(project.Name, processContext.Request.ProjectName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var deactivatedCount = 0;
        foreach (var project in sameNameProjects)
        {
            if (string.Equals(project.Uuid, processContext.TargetProject?.Uuid, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!project.Active && !project.IsLatest)
            {
                continue;
            }

            project.Active = false;
            project.IsLatest = false;
            project.Description = $"{project.Description} \n\nAuto-deactivated by process of '{processContext.Request.ProjectName}' version '{processContext.Request.Version}'";
            // Ensure parent is retained for deactivated siblings to avoid unintended parent dissociation.
            // This is needed because the GetProjectsAsync call does not populate the project.Parent in the returned data,
            // which causes the parent to be set to null in the update request and thus dissociating the parent for all siblings.
            project.Parent = processContext.TargetProject?.Parent;

            var deactivated = await UpdateProjectAsync(
                processContext,
                project,
                cancellationToken);

            if (deactivated is not null)
            {
                deactivatedCount++;
            }
        }

        logger.LogInformation("Phase 3 completed. DeactivatedCount={DeactivatedCount}", deactivatedCount);
        return deactivatedCount;
    }

    // Phase 4: retain only N inactive versions; older inactive versions are deleted.
    private async Task<int> CleanupInactiveProjectsAsync(
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Phase 4 started: cleanup inactive projects.");

        var refreshedProjects = await GetProjectsAsync(processContext.Request.ProjectName, processContext, cancellationToken);
        var inactiveSameName = refreshedProjects
            .Where(project =>
                string.Equals(project.Name, processContext.Request.ProjectName, StringComparison.OrdinalIgnoreCase) &&
                !project.Active &&
                !string.Equals(project.Uuid, processContext.TargetProject?.Uuid, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(project => project.LastBomImport ?? DateTimeOffset.MinValue)
            .ThenByDescending(project => project.Version)
            .ToList();

        if (processContext.Request.CleanInactiveProjects.HasValue == false || processContext.Request.CleanInactiveProjects <= 0)
        {
            logger.LogInformation("Phase 4 deletion skipped because cleanInactiveProject <= 0.");
            return 0;
        }

        var deletedInactiveCount = 0;
        var deleteCandidates = inactiveSameName.Skip(processContext.Request.CleanInactiveProjects.Value).ToList();
        foreach (var candidate in deleteCandidates)
        {
            var deleteResponse = await dependencyTrackClient.DeleteProjectAsync(processContext, candidate.Uuid, cancellationToken);

            if (deleteResponse.IsSuccess)
            {
                deletedInactiveCount++;
            }
            else
            {
                logger.LogWarning(
                    "Failed to delete inactive project during cleanup. ProjectUuid={ProjectUuid}, StatusCode={StatusCode}",
                    candidate.Uuid,
                    deleteResponse.StatusCode);
            }
        }

        return deletedInactiveCount;
    }

    private async Task<List<DependencyTrackProjectDto>> GetProjectsAsync(
        string? searchText,
        ProjectProcessContext processContext,
        CancellationToken cancellationToken)
    {
        const int pageSize = 10;
        var pageNumber = 1;
        var projects = new List<DependencyTrackProjectDto>();

        while (pageNumber <= 100) // safety limit to avoid infinite loop in case of unexpected API behavior; adjust as needed based on expected project count and page size
        {
            var response = await dependencyTrackClient.GetAllProjectsAsync(
                processContext,
                excludeInactive: false,
                onlyRoot: false,
                searchText: searchText,
                pageSize: pageSize,
                pageNumber: pageNumber,
                cancellationToken: cancellationToken);

            if (!response.IsSuccess)
            {
                return [];
            }

            var currentPage = response.Data ?? [];
            if (currentPage.Count == 0)
            {
                break;
            }

            projects.AddRange(currentPage);

            if (currentPage.Count < pageSize)
            {
                break;
            }

            pageNumber++;
        }

        return projects;
    }

    private async Task<DependencyTrackProjectDto?> LookupProjectAsync(
        ProjectProcessContext processContext,
        string name,
        string? version,
        CancellationToken cancellationToken)
    {
        var response = await dependencyTrackClient.LookupProjectAsync(processContext, name, version, cancellationToken);
        if (!response.IsSuccess)
        {
            return null;
        }

        return response.Data;
    }

    private async Task<DependencyTrackProjectDto?> CreateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackCreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var response = await dependencyTrackClient.CreateProjectAsync(processContext, request, cancellationToken);
        if (!response.IsSuccess)
        {
            return null;
        }

        return response.Data
            ?? await LookupProjectAsync(processContext, request.Name, request.Version, cancellationToken);
    }

    private async Task<DependencyTrackProjectDto?> UpdateProjectAsync(
        ProjectProcessContext processContext,
        DependencyTrackProjectDto project,
        CancellationToken cancellationToken)
    {
        var request = new DependencyTrackUpdateProjectRequest
        {
            Uuid = project.Uuid,
            Name = project.Name ?? string.Empty,
            Version = project.Version,
            Active = project.Active,
            IsLatest = project.IsLatest,
            Description = project.Description,
            Parent = project.Parent is null || string.IsNullOrWhiteSpace(project.Parent.Uuid)
                ? null
                : new DependencyTrackProjectReferenceDto { Uuid = project.Parent.Uuid },

        };

        var response = await dependencyTrackClient.UpdateProjectAsync(processContext, request, cancellationToken);

        if (!response.IsSuccess)
        {
            logger.LogWarning(
                "Failed to update project. ProjectUuid={ProjectUuid}, StatusCode={StatusCode}",
                project.Uuid,
                response.StatusCode);
            return null;
        }

        var updated = response.Data;
        if (updated is not null)
        {
            return updated;
        }

        return await LookupProjectAsync(processContext, project.Name ?? string.Empty, project.Version, cancellationToken);
    }
}
