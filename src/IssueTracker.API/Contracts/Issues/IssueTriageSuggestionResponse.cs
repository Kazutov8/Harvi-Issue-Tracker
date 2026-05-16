namespace IssueTracker.API.Contracts.Issues;

public sealed record IssueTriageSuggestionResponse(
    Guid IssueId,
    string Priority,
    IReadOnlyList<IssueLabelResponse> Labels,
    string? AcceptanceCriteria,
    bool IsValid,
    string? ValidationError);
