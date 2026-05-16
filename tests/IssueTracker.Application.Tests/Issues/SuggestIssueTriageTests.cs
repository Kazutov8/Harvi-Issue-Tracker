using IssueTracker.Application.Abstractions;
using IssueTracker.Application.Issues;
using IssueTracker.Domain.Entities;
using Xunit;

namespace IssueTracker.Application.Tests.Issues;

public sealed class SuggestIssueTriageTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidResult_WhenPriorityIsUnknown()
    {
        var issue = Issue.Create(Guid.NewGuid(), "Broken login", null, Guid.NewGuid());
        var useCase = new SuggestIssueTriage(
            new StubIssueRepository(issue),
            new StubLabelRepository([]),
            new StubTriageAgent(new TriageAgentResponse("urgent", [], "Draft")));

        var result = await useCase.ExecuteAsync(issue.Id);

        Assert.False(result.IsValid);
        Assert.Equal("AI returned an invalid priority value.", result.ValidationError);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInvalidResult_WhenLabelIsUnknown()
    {
        var projectId = Guid.NewGuid();
        var issue = Issue.Create(projectId, "Broken login", null, Guid.NewGuid());
        var labels = new[] { Label.Create(projectId, "bug") };
        var useCase = new SuggestIssueTriage(
            new StubIssueRepository(issue),
            new StubLabelRepository(labels),
            new StubTriageAgent(new TriageAgentResponse("high", ["feature"], "Draft")));

        var result = await useCase.ExecuteAsync(issue.Id);

        Assert.False(result.IsValid);
        Assert.Equal("AI suggested an unknown label: 'feature'.", result.ValidationError);
    }

    private sealed class StubIssueRepository(Issue issue) : IIssueRepository
    {
        public Task AddAsync(Issue issue, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Issue?> GetByIdAsync(Guid issueId, CancellationToken cancellationToken = default) => Task.FromResult<Issue?>(issue);
        public Task<IReadOnlyList<Issue>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Issue>>([issue]);
    }

    private sealed class StubLabelRepository(IReadOnlyList<Label> labels) : ILabelRepository
    {
        public Task AddAsync(Label label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Label>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default) => Task.FromResult(labels);
    }

    private sealed class StubTriageAgent(TriageAgentResponse response) : ITriageAgent
    {
        public Task<TriageAgentResponse> SuggestAsync(TriageAgentRequest request, CancellationToken cancellationToken = default) => Task.FromResult(response);
    }
}
