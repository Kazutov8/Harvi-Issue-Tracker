using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence.Repositories;

public sealed class LabelRepository(IssueTrackerDbContext dbContext) : ILabelRepository
{
    public Task AddAsync(Label label, CancellationToken cancellationToken = default)
    {
        return dbContext.Labels.AddAsync(label, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<Label>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Labels
            .Where(label => label.ProjectId == projectId)
            .OrderBy(label => label.Name)
            .ToListAsync(cancellationToken);
    }
}
