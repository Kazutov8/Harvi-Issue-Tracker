using IssueTracker.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence;

public sealed class IssueTrackerDbContext(DbContextOptions<IssueTrackerDbContext> options)
    : DbContext(options), IApplicationDbContext
{
}
