using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence.Repositories;

public sealed class IssueRepository(IssueTrackerDbContext dbContext) : IIssueRepository
{
    public Task AddAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        return dbContext.Issues.AddAsync(issue, cancellationToken).AsTask();
    }

    public Task<Issue?> GetByIdAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        return dbContext.Issues
            .Include(issue => issue.Labels)
            .FirstOrDefaultAsync(issue => issue.Id == issueId, cancellationToken);
    }

    public async Task<IReadOnlyList<Issue>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Issues
            .Include(issue => issue.Labels)
            .Where(issue => issue.ProjectId == projectId)
            .OrderByDescending(issue => issue.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
