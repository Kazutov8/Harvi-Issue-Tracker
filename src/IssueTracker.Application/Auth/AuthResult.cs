namespace IssueTracker.Application.Auth;

public sealed record AuthResult(
    string AccessToken,
    UserDto User);
