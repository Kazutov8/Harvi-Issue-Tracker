using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed class ApplyIssueTriage(
    IIssueRepository issueRepository,
    ILabelRepository labelRepository,
    IApplicationDbContext applicationDbContext)
{
    public async Task<IssueDto> ExecuteAsync(
        Guid issueId,
        IssuePriority priority,
        IReadOnlyCollection<Guid> labelIds,
        string? acceptanceCriteria,
        CancellationToken cancellationToken = default)
    {
        var issue = await issueRepository.GetByIdAsync(issueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue was not found.");

        var availableLabels = await labelRepository.ListByProjectIdAsync(issue.ProjectId, cancellationToken);
        var labelsById = availableLabels.ToDictionary(label => label.Id);
        var selectedLabels = new List<Domain.Entities.Label>();

        foreach (var labelId in labelIds.Distinct())
        {
            if (!labelsById.TryGetValue(labelId, out var label))
            {
                throw new InvalidOperationException("One or more labels are invalid for this project.");
            }

            selectedLabels.Add(label);
        }

        issue.ApplyTriage(priority, selectedLabels, acceptanceCriteria);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return IssueMappings.ToDto(issue);
    }
}
