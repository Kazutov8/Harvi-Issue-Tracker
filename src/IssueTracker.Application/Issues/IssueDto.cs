using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed record IssueDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    IssueStatus Status,
    IssuePriority Priority,
    Guid ReporterUserId,
    Guid? AssigneeUserId,
    string? AcceptanceCriteria,
    bool AcceptanceCriteriaIsAiGenerated,
    DateTime CreatedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyList<IssueLabelDto> Labels);
