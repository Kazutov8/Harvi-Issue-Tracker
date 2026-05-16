using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IssueTracker.API.IntegrationTests.TestHost;
using Xunit;

namespace IssueTracker.API.IntegrationTests.Auth;

public sealed class AuthEndpointsTests
{
    [Fact]
    public async Task Register_CreatesUser_AndReturnsAccessToken()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var request = new RegisterRequest("alice@example.com", "Password123!", "Alice");

        var response = await client.PostAsJsonAsync("/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(request.Email, body.User.Email);
        Assert.Equal(request.DisplayName, body.User.DisplayName);
        Assert.NotEqual(Guid.Empty, body.User.Id);
    }

    [Fact]
    public async Task Login_ReturnsAccessToken_ForRegisteredUser()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var registerRequest = new RegisterRequest("bob@example.com", "Password123!", "Bob");
        await client.PostAsJsonAsync("/auth/register", registerRequest);

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(registerRequest.Email, registerRequest.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(registerRequest.Email, body.User.Email);
        Assert.Equal(registerRequest.DisplayName, body.User.DisplayName);
    }

    [Fact]
    public async Task Me_ReturnsCurrentUser_ForValidBearerToken()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var registerRequest = new RegisterRequest("carol@example.com", "Password123!", "Carol");
        var registerResponse = await client.PostAsJsonAsync("/auth/register", registerRequest);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(user);
        Assert.Equal(auth.User.Id, user.Id);
        Assert.Equal(registerRequest.Email, user.Email);
        Assert.Equal(registerRequest.DisplayName, user.DisplayName);
    }

    [Fact]
    public async Task ListUsers_ReturnsRegisteredUsers_ForAuthorizedRequest()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var firstUser = await client.PostAsJsonAsync("/auth/register", new RegisterRequest("dave@example.com", "Password123!", "Dave"));
        var auth = await firstUser.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        await client.PostAsJsonAsync("/auth/register", new RegisterRequest("erin@example.com", "Password123!", "Erin"));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/auth/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();

        Assert.NotNull(users);
        Assert.True(users.Count >= 2);
        Assert.Contains(users, user => user.Email == "dave@example.com");
        Assert.Contains(users, user => user.Email == "erin@example.com");
    }
}
