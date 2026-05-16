using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Issues;

public sealed record ApplyIssueTriageRequest(
    [Required]
    string Priority,
    [Required]
    IReadOnlyList<Guid> LabelIds,
    string? AcceptanceCriteria);
