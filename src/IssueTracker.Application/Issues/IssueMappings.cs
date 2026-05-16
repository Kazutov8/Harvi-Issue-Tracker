using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Issues;

internal static class IssueMappings
{
    public static IssueDto ToDto(Issue issue)
    {
        return new IssueDto(
            issue.Id,
            issue.ProjectId,
            issue.Title,
            issue.Description,
            issue.Status,
            issue.Priority,
            issue.ReporterUserId,
            issue.AssigneeUserId,
            issue.AcceptanceCriteria,
            issue.AcceptanceCriteriaIsAiGenerated,
            issue.CreatedAtUtc,
            issue.ClosedAtUtc,
            issue.Labels.Select(label => new IssueLabelDto(label.Id, label.Name)).ToList());
    }
}
