using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Auth;

public sealed class LoginUser(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<AuthResult?> ExecuteAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(user.PasswordHash, password))
        {
            return null;
        }

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
