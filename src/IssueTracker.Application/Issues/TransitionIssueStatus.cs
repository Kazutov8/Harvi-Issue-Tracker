using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed class TransitionIssueStatus(
    IIssueRepository issueRepository,
    IApplicationDbContext applicationDbContext)
{
    public async Task<IssueDto> ExecuteAsync(Guid issueId, IssueStatus status, CancellationToken cancellationToken = default)
    {
        var issue = await issueRepository.GetByIdAsync(issueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue was not found.");

        issue.TransitionTo(status);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return IssueMappings.ToDto(issue);
    }
}
