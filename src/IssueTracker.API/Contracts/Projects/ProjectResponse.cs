namespace IssueTracker.API.Contracts.Projects;

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);
