namespace IssueTracker.API.IntegrationTests.Auth;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, UserResponse User);

public sealed record UserResponse(Guid Id, string Email, string DisplayName, DateTime CreatedAtUtc);
