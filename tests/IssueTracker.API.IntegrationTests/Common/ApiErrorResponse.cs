namespace IssueTracker.API.IntegrationTests.Common;

public sealed record ApiErrorResponse(
    int Status,
    string Title,
    string? Detail,
    IReadOnlyDictionary<string, string[]>? Errors);
