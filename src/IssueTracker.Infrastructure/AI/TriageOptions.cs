namespace IssueTracker.Infrastructure.AI;

public sealed class TriageOptions
{
    public const string SectionName = "TriageAI";

    public string BaseUrl { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 60;
}
