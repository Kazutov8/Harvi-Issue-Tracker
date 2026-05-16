namespace IssueTracker.API.Contracts.Issues;

public sealed record ApplyIssueTriageRequest(
    string Priority,
    IReadOnlyList<Guid> LabelIds,
    string? AcceptanceCriteria);
