using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Issues;

public sealed record AssignIssueRequest(
    [Required]
    Guid AssigneeUserId);
