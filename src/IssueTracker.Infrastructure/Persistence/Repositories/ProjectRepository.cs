using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(IssueTrackerDbContext dbContext) : IProjectRepository
{
    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        return dbContext.Projects.AddAsync(project, cancellationToken).AsTask();
    }

    public Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return dbContext.Projects.FirstOrDefaultAsync(project => project.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .OrderBy(project => project.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
