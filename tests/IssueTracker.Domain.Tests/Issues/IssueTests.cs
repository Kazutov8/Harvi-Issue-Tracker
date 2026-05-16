using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Enums;
using Xunit;

namespace IssueTracker.Domain.Tests.Issues;

public sealed class IssueTests
{
    [Fact]
    public void Create_SetsExpectedMvpDefaults()
    {
        var reporterUserId = Guid.NewGuid();

        var issue = Issue.Create(Guid.NewGuid(), "Need backlog item", null, reporterUserId);

        Assert.Equal(IssueStatus.Backlog, issue.Status);
        Assert.Equal(IssuePriority.Medium, issue.Priority);
        Assert.Equal(reporterUserId, issue.ReporterUserId);
        Assert.Null(issue.AssigneeUserId);
        Assert.Null(issue.Description);
        Assert.Empty(issue.Labels);
        Assert.Null(issue.ClosedAtUtc);
    }

    [Fact]
    public void ApplyTriage_UpdatesPriorityLabelsAndAcceptanceCriteria()
    {
        var projectId = Guid.NewGuid();
        var issue = Issue.Create(projectId, "Need backlog item", null, Guid.NewGuid());
        var bugLabel = Label.Create(projectId, "bug");

        issue.ApplyTriage(IssuePriority.High, [bugLabel], "User can submit the fixed form.");

        Assert.Equal(IssuePriority.High, issue.Priority);
        Assert.Single(issue.Labels);
        Assert.Equal(bugLabel.Id, issue.Labels.Single().Id);
        Assert.Equal("User can submit the fixed form.", issue.AcceptanceCriteria);
        Assert.True(issue.AcceptanceCriteriaIsAiGenerated);
    }

    [Fact]
    public void TransitionTo_DoneSetsClosedAt_AndLeavingDoneClearsIt()
    {
        var issue = Issue.Create(Guid.NewGuid(), "Need backlog item", null, Guid.NewGuid());

        issue.TransitionTo(IssueStatus.Done);

        Assert.Equal(IssueStatus.Done, issue.Status);
        Assert.NotNull(issue.ClosedAtUtc);

        issue.TransitionTo(IssueStatus.Todo);

        Assert.Equal(IssueStatus.Todo, issue.Status);
        Assert.Null(issue.ClosedAtUtc);
    }
}
