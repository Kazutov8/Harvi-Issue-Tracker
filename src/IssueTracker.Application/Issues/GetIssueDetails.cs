using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Issues;

public sealed class GetIssueDetails(IIssueRepository issueRepository)
{
    public async Task<IssueDto?> ExecuteAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var issue = await issueRepository.GetByIdAsync(issueId, cancellationToken);
        return issue is null ? null : IssueMappings.ToDto(issue);
    }
}
