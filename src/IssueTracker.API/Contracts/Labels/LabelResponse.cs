namespace IssueTracker.API.Contracts.Labels;

public sealed record LabelResponse(Guid Id, Guid ProjectId, string Name, DateTime CreatedAtUtc);
