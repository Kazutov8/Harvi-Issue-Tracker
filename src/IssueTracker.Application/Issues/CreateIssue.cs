using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Issues;

public sealed class CreateIssue(
    IProjectRepository projectRepository,
    IIssueRepository issueRepository,
    IApplicationDbContext applicationDbContext)
{
    public async Task<IssueDto> ExecuteAsync(
        string projectSlug,
        string title,
        string? description,
        Guid reporterUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetBySlugAsync(projectSlug, cancellationToken)
            ?? throw new InvalidOperationException("Project was not found.");

        var issue = Issue.Create(project.Id, title, description, reporterUserId);

        await issueRepository.AddAsync(issue, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return IssueMappings.ToDto(issue);
    }
}
