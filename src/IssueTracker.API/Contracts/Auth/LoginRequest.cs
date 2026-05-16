using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Auth;

public sealed record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password);
