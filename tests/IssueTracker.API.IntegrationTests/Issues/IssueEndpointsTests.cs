using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using IssueTracker.API.IntegrationTests.Auth;
using IssueTracker.API.IntegrationTests.Projects;
using IssueTracker.Application.Abstractions;
using IssueTracker.API.IntegrationTests.TestHost;
using IssueTracker.Domain.Entities;
using IssueTracker.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IssueTracker.API.IntegrationTests.Issues;

public sealed class IssueEndpointsTests
{
    [Fact]
    public async Task Create_CreatesIssueWithDomainDefaults()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var auth = await RegisterAndAuthenticateAsync(client, "issue-creator@example.com");
        var project = await CreateProjectAsync(client, "Issue Tracker MVP");

        var response = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Login fails", null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.NotNull(body);
        Assert.Equal(project.Id, body.ProjectId);
        Assert.Equal("Login fails", body.Title);
        Assert.Null(body.Description);
        Assert.Equal("backlog", body.Status);
        Assert.Equal("medium", body.Priority);
        Assert.Equal(auth.User.Id, body.ReporterUserId);
        Assert.Null(body.AssigneeUserId);
        Assert.Empty(body.Labels);
    }

    [Fact]
    public async Task GetById_ReturnsIssueDetails()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "issue-reader@example.com");
        var project = await CreateProjectAsync(client, "Support Console");
        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Search is broken", "Results page stays empty."));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.NotNull(createdIssue);

        var response = await client.GetAsync($"/issues/{createdIssue.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.NotNull(body);
        Assert.Equal(createdIssue.Id, body.Id);
        Assert.Equal("Search is broken", body.Title);
        Assert.Equal("Results page stays empty.", body.Description);
    }

    [Fact]
    public async Task List_ReturnsProjectIssuesInNewestFirstOrder()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "issue-list@example.com");
        var project = await CreateProjectAsync(client, "Workflow Board");

        await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("First issue", null));

        await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Second issue", "Later backlog item"));

        var response = await client.GetAsync($"/projects/{project.Slug}/issues");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<IssueResponse>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Equal("Second issue", body[0].Title);
        Assert.Equal("First issue", body[1].Title);
    }

    [Fact]
    public async Task List_HidesDoneIssuesByDefault_AndReturnsThemWhenExplicitlyRequested()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "issue-done-filter@example.com");
        var project = await CreateProjectAsync(client, "Done Filter Board");

        var activeIssue = await CreateIssueAsync(client, project.Slug, "Visible backlog issue", null);
        var doneIssue = await CreateIssueAsync(client, project.Slug, "Completed issue", null);

        await client.PostAsJsonAsync($"/issues/{doneIssue.Id}/transition", new TransitionIssueStatusRequest("done"));

        var defaultResponse = await client.GetAsync($"/projects/{project.Slug}/issues");
        var defaultBody = await defaultResponse.Content.ReadFromJsonAsync<List<IssueResponse>>();

        Assert.Equal(HttpStatusCode.OK, defaultResponse.StatusCode);
        Assert.NotNull(defaultBody);
        Assert.Single(defaultBody);
        Assert.Equal(activeIssue.Id, defaultBody[0].Id);

        var includeDoneResponse = await client.GetAsync($"/projects/{project.Slug}/issues?includeDone=true");
        var includeDoneBody = await includeDoneResponse.Content.ReadFromJsonAsync<List<IssueResponse>>();

        Assert.Equal(HttpStatusCode.OK, includeDoneResponse.StatusCode);
        Assert.NotNull(includeDoneBody);
        Assert.Equal(2, includeDoneBody.Count);

        var doneOnlyResponse = await client.GetAsync($"/projects/{project.Slug}/issues?status=done");
        var doneOnlyBody = await doneOnlyResponse.Content.ReadFromJsonAsync<List<IssueResponse>>();

        Assert.Equal(HttpStatusCode.OK, doneOnlyResponse.StatusCode);
        Assert.NotNull(doneOnlyBody);
        Assert.Single(doneOnlyBody);
        Assert.Equal(doneIssue.Id, doneOnlyBody[0].Id);
    }

    [Fact]
    public async Task List_FiltersByAssigneeLabelStatusAndTextQuery()
    {
        Guid bugLabelId = Guid.Empty;
        Guid frontendLabelId = Guid.Empty;

        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var auth = await RegisterAndAuthenticateAsync(client, "issue-combined-filter@example.com");
        var project = await CreateProjectAsync(client, "Combined Filters Board");
        var assignee = await RegisterUserAsync(client, "filter-worker@example.com", "Filter Worker");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IssueTrackerDbContext>();
            var bugLabel = Label.Create(project.Id, "bug");
            var frontendLabel = Label.Create(project.Id, "frontend");
            bugLabelId = bugLabel.Id;
            frontendLabelId = frontendLabel.Id;
            dbContext.Labels.AddRange(bugLabel, frontendLabel);
            await dbContext.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var matchingIssue = await CreateIssueAsync(client, project.Slug, "Search login regression", "Frontend login screen crashes.");
        var otherIssue = await CreateIssueAsync(client, project.Slug, "Background sync task", "Back office import issue.");

        await client.PostAsJsonAsync(
            $"/issues/{matchingIssue.Id}/apply-triage-suggestion",
            new ApplyIssueTriageRequest("high", [bugLabelId, frontendLabelId], "Fix login page."));
        await client.PostAsJsonAsync($"/issues/{matchingIssue.Id}/assign", new AssignIssueRequest(assignee.Id));
        await client.PostAsJsonAsync($"/issues/{matchingIssue.Id}/transition", new TransitionIssueStatusRequest("todo"));

        await client.PostAsJsonAsync(
            $"/issues/{otherIssue.Id}/apply-triage-suggestion",
            new ApplyIssueTriageRequest("medium", [bugLabelId], "Review background job."));

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["status"] = "todo";
        query["assigneeUserId"] = assignee.Id.ToString();
        query["query"] = "login";
        query.Add("labelIds", frontendLabelId.ToString());

        var response = await client.GetAsync($"/projects/{project.Slug}/issues?{query}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<IssueResponse>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal(matchingIssue.Id, body[0].Id);
    }

    [Fact]
    public async Task SuggestAiTriage_ReturnsValidatedSuggestion()
    {
        Guid projectId = Guid.Empty;

        await using var factory = new ApiTestFactory(
            services =>
            {
                services.AddScoped<ITriageAgent>(_ => new FakeTriageAgent(_ =>
                    new TriageAgentResponse("high", ["bug"], "User can log in after entering valid credentials.")));
            });

        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "issue-triage@example.com");
        var project = await CreateProjectAsync(client, "Triaged Project");
        projectId = project.Id;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IssueTrackerDbContext>();
            dbContext.Labels.Add(Label.Create(projectId, "bug"));
            await dbContext.SaveChangesAsync();
        }

        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Login is broken", "Users cannot enter the dashboard."));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(createdIssue);

        var response = await client.PostAsync($"/issues/{createdIssue.Id}/ai-suggest", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IssueTriageSuggestionResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsValid);
        Assert.Equal("high", body.Priority);
        Assert.Single(body.Labels);
        Assert.Equal("bug", body.Labels[0].Name);
    }

    [Fact]
    public async Task SuggestAiTriage_ReturnsBadGatewayWhenProviderFails()
    {
        await using var factory = new ApiTestFactory(services =>
        {
            services.AddScoped<ITriageAgent>(_ => new ThrowingTriageAgent());
        });

        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "issue-triage-fail@example.com");
        var project = await CreateProjectAsync(client, "Triage Failure Project");
        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("API timeout", null));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(createdIssue);

        var response = await client.PostAsync($"/issues/{createdIssue.Id}/ai-suggest", null);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task ApplyTriageSuggestion_UpdatesIssuePriorityLabelsAndAcceptanceCriteria()
    {
        Guid bugLabelId = Guid.Empty;

        await using var factory = new ApiTestFactory();

        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "triage-apply@example.com");
        var project = await CreateProjectAsync(client, "Phase 6 Project");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IssueTrackerDbContext>();
            var label = Label.Create(project.Id, "bug");
            bugLabelId = label.Id;
            dbContext.Labels.Add(label);
            await dbContext.SaveChangesAsync();
        }

        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Broken login", null));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(createdIssue);

        var response = await client.PostAsJsonAsync(
            $"/issues/{createdIssue.Id}/apply-triage-suggestion",
            new ApplyIssueTriageRequest("high", [bugLabelId], "User can log in successfully."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(body);
        Assert.Equal("high", body.Priority);
        Assert.Single(body.Labels);
        Assert.Equal("bug", body.Labels[0].Name);
        Assert.Equal("User can log in successfully.", body.AcceptanceCriteria);
        Assert.True(body.AcceptanceCriteriaIsAiGenerated);
    }

    [Fact]
    public async Task AssignIssue_UpdatesAssignee_WhenUserExists()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var auth = await RegisterAndAuthenticateAsync(client, "assigner@example.com");
        var project = await CreateProjectAsync(client, "Assignments");
        var assigneeRegisterResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest("worker@example.com", "Password123!", "Worker"));
        var assigneeAuth = await assigneeRegisterResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(assigneeAuth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Broken login", null));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(createdIssue);

        var response = await client.PostAsJsonAsync(
            $"/issues/{createdIssue.Id}/assign",
            new AssignIssueRequest(assigneeAuth.User.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(body);
        Assert.Equal(assigneeAuth.User.Id, body.AssigneeUserId);
    }

    [Fact]
    public async Task TransitionIssueStatus_UpdatesStatusAndClosedAt()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "transition@example.com");
        var project = await CreateProjectAsync(client, "Workflow States");
        var createResponse = await client.PostAsJsonAsync(
            $"/projects/{project.Slug}/issues",
            new CreateIssueRequest("Broken login", null));

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(createdIssue);

        var todoResponse = await client.PostAsJsonAsync(
            $"/issues/{createdIssue.Id}/transition",
            new TransitionIssueStatusRequest("todo"));

        Assert.Equal(HttpStatusCode.OK, todoResponse.StatusCode);

        var todoBody = await todoResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(todoBody);
        Assert.Equal("todo", todoBody.Status);
        Assert.Null(todoBody.ClosedAtUtc);

        var doneResponse = await client.PostAsJsonAsync(
            $"/issues/{createdIssue.Id}/transition",
            new TransitionIssueStatusRequest("done"));

        Assert.Equal(HttpStatusCode.OK, doneResponse.StatusCode);

        var doneBody = await doneResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(doneBody);
        Assert.Equal("done", doneBody.Status);
        Assert.NotNull(doneBody.ClosedAtUtc);

        var reopenResponse = await client.PostAsJsonAsync(
            $"/issues/{createdIssue.Id}/transition",
            new TransitionIssueStatusRequest("todo"));

        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);

        var reopenedBody = await reopenResponse.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(reopenedBody);
        Assert.Equal("todo", reopenedBody.Status);
        Assert.Null(reopenedBody.ClosedAtUtc);
    }

    private sealed class ThrowingTriageAgent : ITriageAgent
    {
        public Task<TriageAgentResponse> SuggestAsync(TriageAgentRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("AI provider request failed.");
        }
    }

    private static async Task<AuthResponse> RegisterAndAuthenticateAsync(HttpClient client, string email)
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, "Password123!", "Issue User"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        return auth;
    }

    private static async Task<UserResponse> RegisterUserAsync(HttpClient client, string email, string displayName)
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, "Password123!", displayName));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);

        return auth.User;
    }

    private static async Task<IssueResponse> CreateIssueAsync(HttpClient client, string projectSlug, string title, string? description)
    {
        var response = await client.PostAsJsonAsync(
            $"/projects/{projectSlug}/issues",
            new CreateIssueRequest(title, description));

        var issue = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.NotNull(issue);

        return issue;
    }

    private static async Task<ProjectResponse> CreateProjectAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(name));
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();

        Assert.NotNull(project);

        return project;
    }
}
