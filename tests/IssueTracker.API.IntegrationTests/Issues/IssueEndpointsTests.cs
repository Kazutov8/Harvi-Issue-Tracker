using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IssueTracker.API.IntegrationTests.Auth;
using IssueTracker.API.IntegrationTests.Projects;
using IssueTracker.API.IntegrationTests.TestHost;
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

    private static async Task<ProjectResponse> CreateProjectAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(name));
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();

        Assert.NotNull(project);

        return project;
    }
}
