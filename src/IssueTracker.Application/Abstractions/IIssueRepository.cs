using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Abstractions;

public interface IIssueRepository
{
    Task AddAsync(Issue issue, CancellationToken cancellationToken = default);

    Task<Issue?> GetByIdAsync(Guid issueId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Issue>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
