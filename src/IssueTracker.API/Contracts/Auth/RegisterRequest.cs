using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Auth;

public sealed record RegisterRequest(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    [MinLength(4)]
    string Password,
    [Required]
    [MinLength(2)]
    string DisplayName);
