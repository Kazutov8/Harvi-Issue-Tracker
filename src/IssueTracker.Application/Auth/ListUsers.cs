using IssueTracker.Application.Abstractions;

namespace IssueTracker.Application.Auth;

public sealed class ListUsers(IUserRepository userRepository)
{
    public async Task<IReadOnlyList<UserDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.ListAsync(cancellationToken);

        return users
            .Select(user => new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc))
            .ToList();
    }
}
