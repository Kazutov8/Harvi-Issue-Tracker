namespace IssueTracker.Application.Labels;

public sealed record ProjectLabelDto(Guid Id, Guid ProjectId, string Name, string NormalizedName, DateTime CreatedAtUtc);
