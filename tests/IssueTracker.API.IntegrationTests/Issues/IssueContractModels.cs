namespace IssueTracker.API.IntegrationTests.Issues;

public sealed record CreateIssueRequest(string Title, string? Description);

public sealed record ApplyIssueTriageRequest(string Priority, IReadOnlyList<Guid> LabelIds, string? AcceptanceCriteria);

public sealed record AssignIssueRequest(Guid AssigneeUserId);

public sealed record TransitionIssueStatusRequest(string Status);

public sealed record IssueLabelResponse(Guid Id, string Name);

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

public sealed record IssueTriageSuggestionResponse(
    Guid IssueId,
    string Priority,
    IReadOnlyList<IssueLabelResponse> Labels,
    string? AcceptanceCriteria,
    bool IsValid,
    string? ValidationError);
