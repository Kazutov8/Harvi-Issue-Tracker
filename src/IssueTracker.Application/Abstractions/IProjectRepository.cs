using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Abstractions;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
}
