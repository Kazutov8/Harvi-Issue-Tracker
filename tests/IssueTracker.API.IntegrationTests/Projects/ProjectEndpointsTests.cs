using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IssueTracker.API.IntegrationTests.Auth;
using IssueTracker.API.IntegrationTests.Common;
using IssueTracker.API.IntegrationTests.TestHost;
using Xunit;

namespace IssueTracker.API.IntegrationTests.Projects;

public sealed class ProjectEndpointsTests
{
    [Fact]
    public async Task Create_CreatesProject_ForAuthenticatedUser()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var auth = await RegisterAndAuthenticateAsync(client, "project-owner@example.com");

        var response = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("Internal Tools"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ProjectResponse>();

        Assert.NotNull(body);
        Assert.Equal("Internal Tools", body.Name);
        Assert.Equal("internal-tools", body.Slug);
        Assert.Equal(auth.User.Id, body.CreatedByUserId);
        Assert.NotEqual(Guid.Empty, body.Id);
    }

    [Fact]
    public async Task List_ReturnsProjects_ForAuthenticatedUser()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "project-reader@example.com");
        await client.PostAsJsonAsync("/projects", new CreateProjectRequest("Alpha Project"));
        await client.PostAsJsonAsync("/projects", new CreateProjectRequest("Beta Project"));

        var response = await client.GetAsync("/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Collection(
            body,
            first => Assert.Equal("alpha-project", first.Slug),
            second => Assert.Equal("beta-project", second.Slug));
    }

    [Fact]
    public async Task Create_ReturnsValidationError_WhenNameIsMissing()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        await RegisterAndAuthenticateAsync(client, "project-invalid@example.com");

        var response = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal(400, body.Status);
        Assert.NotNull(body.Errors);
        Assert.Contains("Name", body.Errors.Keys);
    }

    private static async Task<AuthResponse> RegisterAndAuthenticateAsync(HttpClient client, string email)
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, "Password123!", "Project User"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        return auth;
    }
}
