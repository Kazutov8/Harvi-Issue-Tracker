using IssueTracker.Application.Abstractions;

namespace IssueTracker.API.IntegrationTests.Issues;

public sealed class FakeTriageAgent(Func<TriageAgentRequest, TriageAgentResponse> factory) : ITriageAgent
{
    public Task<TriageAgentResponse> SuggestAsync(TriageAgentRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(factory(request));
    }
}
