using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Issues;

public sealed record CreateIssueRequest(
    [Required]
    [MinLength(2)]
    string Title,
    string? Description);
