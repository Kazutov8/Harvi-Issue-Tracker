namespace IssueTracker.API.Contracts.Auth;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAtUtc);
