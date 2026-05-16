using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IssueTracker.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(project => project.Slug)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(project => project.CreatedByUserId)
            .IsRequired();

        builder.Property(project => project.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(project => project.Slug)
            .IsUnique();
    }
}
