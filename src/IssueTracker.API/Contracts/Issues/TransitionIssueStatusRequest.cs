using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Issues;

public sealed record TransitionIssueStatusRequest(
    [Required]
    string Status);
