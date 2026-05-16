namespace IssueTracker.Domain.Entities;

public sealed class Project
{
    private Project()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Project Create(string name, string slug, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Project slug is required.", nameof(slug));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Creator user id is required.", nameof(createdByUserId));
        }

        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
