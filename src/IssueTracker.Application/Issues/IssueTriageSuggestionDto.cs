using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed record IssueTriageSuggestionDto(
    Guid IssueId,
    IssuePriority Priority,
    IReadOnlyList<IssueLabelDto> Labels,
    string? AcceptanceCriteria,
    bool IsValid,
    string? ValidationError);
