using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Auth;

public sealed class RegisterUser(
    IUserRepository userRepository,
    IApplicationDbContext applicationDbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<AuthResult> ExecuteAsync(string email, string password, string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var existingUser = await userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var passwordHash = passwordHasher.HashPassword(password);
        var user = User.Create(email, normalizedEmail, displayName, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenGenerator.GenerateToken(user.Id, user.Email);

        return new AuthResult(
            accessToken,
            new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
