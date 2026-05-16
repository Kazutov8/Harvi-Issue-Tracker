using IssueTracker.Domain.Enums;

namespace IssueTracker.Domain.Entities;

public sealed class Issue
{
    private readonly List<Label> _labels = [];

    private Issue()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IssueStatus Status { get; private set; }

    public IssuePriority Priority { get; private set; }

    public Guid ReporterUserId { get; private set; }

    public Guid? AssigneeUserId { get; private set; }

    public string? AcceptanceCriteria { get; private set; }

    public bool AcceptanceCriteriaIsAiGenerated { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<Label> Labels => _labels;

    public static Issue Create(Guid projectId, string title, string? description, Guid reporterUserId)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Issue title is required.", nameof(title));
        }

        if (reporterUserId == Guid.Empty)
        {
            throw new ArgumentException("Reporter user id is required.", nameof(reporterUserId));
        }

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        return new Issue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title.Trim(),
            Description = trimmedDescription,
            Status = IssueStatus.Backlog,
            Priority = IssuePriority.Medium,
            ReporterUserId = reporterUserId,
            AssigneeUserId = null,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
