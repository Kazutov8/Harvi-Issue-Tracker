namespace IssueTracker.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);
