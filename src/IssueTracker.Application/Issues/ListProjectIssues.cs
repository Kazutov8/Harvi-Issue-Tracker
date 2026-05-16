using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Issues;

public sealed class ListProjectIssues(IProjectRepository projectRepository, IIssueRepository issueRepository)
{
    public async Task<IReadOnlyList<IssueDto>> ExecuteAsync(string projectSlug, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetBySlugAsync(projectSlug, cancellationToken)
            ?? throw new InvalidOperationException("Project was not found.");

        var issues = await issueRepository.ListByProjectIdAsync(project.Id, cancellationToken);
        return issues.Select(IssueMappings.ToDto).ToList();
    }
}
