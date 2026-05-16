namespace IssueTracker.API.Contracts.Issues;

public sealed record IssueResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid ReporterUserId,
    Guid? AssigneeUserId,
    string? AcceptanceCriteria,
    bool AcceptanceCriteriaIsAiGenerated,
    DateTime CreatedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyList<IssueLabelResponse> Labels);
