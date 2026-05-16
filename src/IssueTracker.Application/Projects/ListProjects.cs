using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Projects;

public sealed class ListProjects(IProjectRepository projectRepository)
{
    public async Task<IReadOnlyList<ProjectDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.ListAsync(cancellationToken);

        return projects
            .Select(project => new ProjectDto(
                project.Id,
                project.Name,
                project.Slug,
                project.CreatedByUserId,
                project.CreatedAtUtc))
            .ToList();
    }
}
