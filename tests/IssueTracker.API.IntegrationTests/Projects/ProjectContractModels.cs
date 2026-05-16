namespace IssueTracker.API.IntegrationTests.Projects;

public sealed record CreateProjectRequest(string Name);

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);
