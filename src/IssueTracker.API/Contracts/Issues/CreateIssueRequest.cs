namespace IssueTracker.API.Contracts.Issues;

public sealed record CreateIssueRequest(string Title, string? Description);
