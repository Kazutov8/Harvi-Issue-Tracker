using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Labels;

public sealed class ListProjectLabels(IProjectRepository projectRepository, ILabelRepository labelRepository)
{
    public async Task<IReadOnlyList<ProjectLabelDto>> ExecuteAsync(string projectSlug, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetBySlugAsync(projectSlug, cancellationToken)
            ?? throw new InvalidOperationException("Project was not found.");

        var labels = await labelRepository.ListByProjectIdAsync(project.Id, cancellationToken);

        return labels
            .Select(label => new ProjectLabelDto(label.Id, label.ProjectId, label.Name, label.NormalizedName, label.CreatedAtUtc))
            .ToList();
    }
}
