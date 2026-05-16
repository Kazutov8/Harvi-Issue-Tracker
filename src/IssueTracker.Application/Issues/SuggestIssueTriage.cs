using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Enums;

namespace IssueTracker.Application.Issues;

public sealed class SuggestIssueTriage(
    IIssueRepository issueRepository,
    ILabelRepository labelRepository,
    ITriageAgent triageAgent)
{
    public async Task<IssueTriageSuggestionDto> ExecuteAsync(Guid issueId, CancellationToken cancellationToken = default)
    {
        var issue = await issueRepository.GetByIdAsync(issueId, cancellationToken)
            ?? throw new InvalidOperationException("Issue was not found.");

        var availableLabels = await labelRepository.ListByProjectIdAsync(issue.ProjectId, cancellationToken);

        var triageResponse = await triageAgent.SuggestAsync(
            new TriageAgentRequest(
                issue.Title,
                issue.Description,
                availableLabels.Select(label => label.Name).ToList()),
            cancellationToken);

        if (!Enum.TryParse<IssuePriority>(triageResponse.Priority, true, out var priority))
        {
            return new IssueTriageSuggestionDto(
                issue.Id,
                IssuePriority.Medium,
                [],
                null,
                false,
                "AI returned an invalid priority value.");
        }

        var labelsByNormalizedName = availableLabels.ToDictionary(
            label => label.NormalizedName,
            label => label,
            StringComparer.Ordinal);

        var selectedLabels = new List<IssueLabelDto>();

        foreach (var suggestedLabel in triageResponse.Labels)
        {
            var normalizedName = suggestedLabel.Trim().ToUpperInvariant();

            if (!labelsByNormalizedName.TryGetValue(normalizedName, out var label))
            {
                return new IssueTriageSuggestionDto(
                    issue.Id,
                    priority,
                    [],
                    null,
                    false,
                    $"AI suggested an unknown label: '{suggestedLabel}'.");
            }

            if (selectedLabels.All(existing => existing.Id != label.Id))
            {
                selectedLabels.Add(new IssueLabelDto(label.Id, label.Name));
            }
        }

        var acceptanceCriteria = string.IsNullOrWhiteSpace(triageResponse.AcceptanceCriteria)
            ? null
            : triageResponse.AcceptanceCriteria.Trim();

        return new IssueTriageSuggestionDto(
            issue.Id,
            priority,
            selectedLabels,
            acceptanceCriteria,
            true,
            null);
    }
}
