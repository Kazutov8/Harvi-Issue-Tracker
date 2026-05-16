using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Infrastructure.Persistence;

public sealed class IssueTrackerDbContext(DbContextOptions<IssueTrackerDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Issue> Issues => Set<Issue>();

    public DbSet<Label> Labels => Set<Label>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IssueTrackerDbContext).Assembly);
    }
}
