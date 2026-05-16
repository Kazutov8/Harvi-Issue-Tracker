using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IssueTracker.Infrastructure.Persistence.Configurations;

public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");

        builder.HasKey(issue => issue.Id);

        builder.Property(issue => issue.ProjectId)
            .IsRequired();

        builder.Property(issue => issue.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(issue => issue.Description)
            .HasMaxLength(4000);

        builder.Property(issue => issue.Status)
            .IsRequired();

        builder.Property(issue => issue.Priority)
            .IsRequired();

        builder.Property(issue => issue.ReporterUserId)
            .IsRequired();

        builder.Property(issue => issue.AssigneeUserId);

        builder.Property(issue => issue.AcceptanceCriteria)
            .HasMaxLength(4000);

        builder.Property(issue => issue.AcceptanceCriteriaIsAiGenerated)
            .IsRequired();

        builder.Property(issue => issue.CreatedAtUtc)
            .IsRequired();

        builder.Property(issue => issue.ClosedAtUtc);

        builder.HasMany(issue => issue.Labels)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "IssueLabels",
                right => right
                    .HasOne<Label>()
                    .WithMany()
                    .HasForeignKey("LabelId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Issue>()
                    .WithMany()
                    .HasForeignKey("IssueId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("IssueLabels");
                    join.HasKey("IssueId", "LabelId");
                });

        builder.HasIndex(issue => new { issue.ProjectId, issue.CreatedAtUtc });
    }
}
