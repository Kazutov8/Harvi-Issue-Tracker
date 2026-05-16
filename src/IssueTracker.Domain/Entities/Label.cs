namespace IssueTracker.Domain.Entities;

public sealed class Label
{
    private Label()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static Label Create(Guid projectId, string name)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Label name is required.", nameof(name));
        }

        var trimmedName = name.Trim();

        return new Label
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = trimmedName,
            NormalizedName = trimmedName.ToUpperInvariant(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
