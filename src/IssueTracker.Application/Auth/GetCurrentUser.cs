using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Auth;

public sealed class GetCurrentUser(IUserRepository userRepository)
{
    public async Task<UserDto?> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? null
            : new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc);
    }
}
