namespace IssueTracker.API.Contracts.Auth;

public sealed record LoginRequest(
    string Email,
    string Password);
