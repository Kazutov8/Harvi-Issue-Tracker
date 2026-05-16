using IssueTracker.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
