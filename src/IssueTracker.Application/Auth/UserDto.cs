namespace IssueTracker.Application.Auth;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    DateTime CreatedAtUtc);
