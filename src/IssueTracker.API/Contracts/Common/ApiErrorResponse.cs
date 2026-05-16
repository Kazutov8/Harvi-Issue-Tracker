namespace IssueTracker.API.Contracts.Common;

public sealed record ApiErrorResponse(
    int Status,
    string Title,
    string? Detail,
    IDictionary<string, string[]>? Errors = null);
