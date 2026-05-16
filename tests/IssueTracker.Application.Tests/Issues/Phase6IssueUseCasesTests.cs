using IssueTracker.Application.Abstractions;
using IssueTracker.Application.Issues;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Enums;
using Xunit;

namespace IssueTracker.Application.Tests.Issues;

public sealed class Phase6IssueUseCasesTests
{
    [Fact]
    public async Task ApplyIssueTriage_RejectsLabelFromAnotherProject()
    {
        var issueProjectId = Guid.NewGuid();
        var issue = Issue.Create(issueProjectId, "Broken login", null, Guid.NewGuid());
        var foreignLabel = Label.Create(Guid.NewGuid(), "bug");
        var applicationDbContext = new StubApplicationDbContext();
        var useCase = new ApplyIssueTriage(
            new StubIssueRepository(issue),
            new StubLabelRepository([foreignLabel]),
            applicationDbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(issue.Id, IssuePriority.High, [foreignLabel.Id], "Draft"));

        Assert.Equal("Issue labels must belong to the same project.", exception.Message);
    }

    [Fact]
    public async Task AssignIssue_Throws_WhenAssigneeDoesNotExist()
    {
        var issue = Issue.Create(Guid.NewGuid(), "Broken login", null, Guid.NewGuid());
        var useCase = new AssignIssue(
            new StubIssueRepository(issue),
            new StubUserRepository(null, []),
            new StubApplicationDbContext());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(issue.Id, Guid.NewGuid()));

        Assert.Equal("Assignee was not found.", exception.Message);
    }

    [Fact]
    public async Task TransitionIssueStatus_SetsAndClearsClosedAt()
    {
        var issue = Issue.Create(Guid.NewGuid(), "Broken login", null, Guid.NewGuid());
        var useCase = new TransitionIssueStatus(new StubIssueRepository(issue), new StubApplicationDbContext());

        await useCase.ExecuteAsync(issue.Id, IssueStatus.Done);
        Assert.NotNull(issue.ClosedAtUtc);

        await useCase.ExecuteAsync(issue.Id, IssueStatus.InProgress);
        Assert.Null(issue.ClosedAtUtc);
    }

    private sealed class StubApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
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

    private sealed class StubUserRepository(User? userById, IReadOnlyList<User> users) : IUserRepository
    {
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(userById);
        public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) => Task.FromResult<User?>(users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail));
        public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult(users);
    }
}
