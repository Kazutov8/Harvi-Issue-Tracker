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
}
