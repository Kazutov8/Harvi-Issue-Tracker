namespace IssueTracker.Application.Abstractions;

public interface ITriageAgent
{
    Task<TriageAgentResponse> SuggestAsync(TriageAgentRequest request, CancellationToken cancellationToken = default);
}

public sealed record TriageAgentRequest(
    string Title,
    string? Description,
    IReadOnlyList<string> AvailableLabels);

public sealed record TriageAgentResponse(
    string? Priority,
    IReadOnlyList<string> Labels,
    string? AcceptanceCriteria);
