using IssueTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IssueTracker.Infrastructure.Persistence.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("Labels");

        builder.HasKey(label => label.Id);

        builder.Property(label => label.ProjectId)
            .IsRequired();

        builder.Property(label => label.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(label => label.NormalizedName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(label => label.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(label => new { label.ProjectId, label.NormalizedName })
            .IsUnique();
    }
}
