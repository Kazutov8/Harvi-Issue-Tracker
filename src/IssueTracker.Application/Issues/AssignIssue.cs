using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Issues;

public sealed class AssignIssue(
    IIssueRepository issueRepository,
    IUserRepository userRepository,
    IApplicationDbContext applicationDbContext)
{
    public async Task<IssueDto> ExecuteAsync(Guid issueId, Guid assigneeUserId, CancellationToken cancellationToken = default)
    {
        var issue = await issueRepository.GetByIdAsync(issueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue was not found.");

        var assignee = await userRepository.GetByIdAsync(assigneeUserId, cancellationToken);

        if (assignee is null)
        {
            throw new InvalidOperationException("Assignee was not found.");
        }

        issue.AssignTo(assignee.Id);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return IssueMappings.ToDto(issue);
    }
}
