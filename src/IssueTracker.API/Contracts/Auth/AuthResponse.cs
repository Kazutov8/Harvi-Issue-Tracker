namespace IssueTracker.API.Contracts.Auth;

public sealed record AuthResponse(
    string AccessToken,
    UserResponse User);
