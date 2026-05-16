using IssueTracker.Application.Abstractions;
using IssueTracker.Application.Issues;
using IssueTracker.Domain.Entities;
using IssueTracker.Domain.Enums;
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

    public async Task<IReadOnlyList<Issue>> ListByProjectIdAsync(
        Guid projectId,
        ProjectIssuesQuery query,
        CancellationToken cancellationToken = default)
    {
        var issuesQuery = dbContext.Issues
            .Include(issue => issue.Labels)
            .Where(issue => issue.ProjectId == projectId);

        if (query.Status is not null)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Status == query.Status.Value);
        }
        else if (!query.IncludeDone)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Status != IssueStatus.Done);
        }

        if (query.AssigneeUserId is not null)
        {
            issuesQuery = issuesQuery.Where(issue => issue.AssigneeUserId == query.AssigneeUserId.Value);
        }

        if (query.LabelIds.Count > 0)
        {
            issuesQuery = issuesQuery.Where(issue => issue.Labels.Any(label => query.LabelIds.Contains(label.Id)));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            issuesQuery = issuesQuery.Where(issue =>
                EF.Functions.Like(issue.Title, $"%{search}%") ||
                (issue.Description != null && EF.Functions.Like(issue.Description, $"%{search}%")));
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        return await issuesQuery
            .OrderByDescending(issue => issue.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
