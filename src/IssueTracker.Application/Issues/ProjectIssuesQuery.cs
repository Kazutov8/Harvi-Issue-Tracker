using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed record ProjectIssuesQuery(
    IssueStatus? Status,
    Guid? AssigneeUserId,
    IReadOnlyList<Guid> LabelIds,
    string? Search,
    int Page,
    int PageSize,
    bool IncludeDone)
{
    public static ProjectIssuesQuery Default => new(null, null, [], null, 1, 20, false);
}
