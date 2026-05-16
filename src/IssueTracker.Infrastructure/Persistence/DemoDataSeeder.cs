using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Enums;

namespace IssueTracker.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IssueTrackerDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.Users.Any() || dbContext.Projects.Any() || dbContext.Issues.Any())
        {
            return;
        }

        var user = User.Create(
            "demo@example.com",
            "DEMO@EXAMPLE.COM",
            "Demo User",
            BCrypt.Net.BCrypt.HashPassword("Password123!"));

        var project = Project.Create("Demo Workspace", "demo-workspace", user.Id);
        var bugLabel = Label.Create(project.Id, "bug");
        var uxLabel = Label.Create(project.Id, "ux");
        var aiLabel = Label.Create(project.Id, "ai-triage");

        var issueOne = Issue.Create(project.Id, "Login button does nothing", "Clicking login on Safari leaves the form idle.", user.Id);
        issueOne.ApplyTriage(IssuePriority.High, [bugLabel, uxLabel], "1. User clicks login and sees progress.\n2. Successful login opens dashboard.\n3. Failed login shows a readable error.");
        issueOne.AssignTo(user.Id);
        issueOne.TransitionTo(IssueStatus.Todo);

        var issueTwo = Issue.Create(project.Id, "Clarify AI triage empty-label behavior", "When no labels exist, the model must return an empty array instead of placeholders.", user.Id);
        issueTwo.ApplyTriage(IssuePriority.Medium, [aiLabel], "1. Empty label set is sent to provider.\n2. Response labels field is [].\n3. Placeholder labels are rejected.");

        dbContext.Users.Add(user);
        dbContext.Projects.Add(project);
        dbContext.Labels.AddRange(bugLabel, uxLabel, aiLabel);
        dbContext.Issues.AddRange(issueOne, issueTwo);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
