using System.Security.Claims;
using IssueTracker.API.Contracts.Issues;
using IssueTracker.API.Contracts.Labels;
using IssueTracker.Application.Issues;
using IssueTracker.Application.Labels;
using IssueTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.API.Controllers;

[ApiController]
[Authorize]
public sealed class IssuesController(
    CreateIssue createIssue,
    GetIssueDetails getIssueDetails,
    ListProjectIssues listProjectIssues,
    ListProjectLabels listProjectLabels,
    SuggestIssueTriage suggestIssueTriage,
    ApplyIssueTriage applyIssueTriage,
    AssignIssue assignIssue,
    TransitionIssueStatus transitionIssueStatus) : ControllerBase
{
    [HttpPost("projects/{projectSlug}/issues")]
    public async Task<ActionResult<IssueResponse>> Create(
        string projectSlug,
        CreateIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ValidationProblem("Issue title is required.");
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var reporterUserId))
        {
            return Unauthorized();
        }

        try
        {
            var issue = await createIssue.ExecuteAsync(projectSlug, request.Title, request.Description, reporterUserId, cancellationToken);
            return Ok(ToResponse(issue));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("projects/{projectSlug}/issues")]
    public async Task<ActionResult<IReadOnlyList<IssueResponse>>> List(
        string projectSlug,
        [FromQuery] string? status,
        [FromQuery] Guid? assigneeUserId,
        [FromQuery] Guid[]? labelIds,
        [FromQuery] string? query,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool includeDone,
        CancellationToken cancellationToken)
    {
        if (!TryParseStatus(status, out var parsedStatus))
        {
            return ValidationProblem("Status value is invalid.");
        }

        try
        {
            var issues = await listProjectIssues.ExecuteAsync(
                projectSlug,
                new ProjectIssuesQuery(
                    parsedStatus,
                    assigneeUserId,
                    labelIds ?? [],
                    query,
                    page,
                    pageSize,
                    includeDone),
                cancellationToken);

            return Ok(issues.Select(ToResponse).ToList());
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("projects/{projectSlug}/labels")]
    public async Task<ActionResult<IReadOnlyList<LabelResponse>>> ListLabels(string projectSlug, CancellationToken cancellationToken)
    {
        try
        {
            var labels = await listProjectLabels.ExecuteAsync(projectSlug, cancellationToken);
            return Ok(labels.Select(label => new LabelResponse(label.Id, label.ProjectId, label.Name, label.CreatedAtUtc)).ToList());
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("issues/{id:guid}")]
    public async Task<ActionResult<IssueResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var issue = await getIssueDetails.ExecuteAsync(id, cancellationToken);

        if (issue is null)
        {
            return NotFound(new { message = "Issue was not found." });
        }

        return Ok(ToResponse(issue));
    }

    [HttpPost("issues/{id:guid}/ai-suggest")]
    public async Task<ActionResult<IssueTriageSuggestionResponse>> Suggest(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var suggestion = await suggestIssueTriage.ExecuteAsync(id, cancellationToken);

            if (!suggestion.IsValid)
            {
                return UnprocessableEntity(ToSuggestionResponse(suggestion));
            }

            return Ok(ToSuggestionResponse(suggestion));
        }
        catch (InvalidOperationException exception) when (exception.Message == "Issue was not found.")
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = exception.Message });
        }
    }

    [HttpPost("issues/{id:guid}/apply-triage-suggestion")]
    public async Task<ActionResult<IssueResponse>> ApplyTriage(
        Guid id,
        ApplyIssueTriageRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IssuePriority>(request.Priority, true, out var priority))
        {
            return ValidationProblem("Priority value is invalid.");
        }

        try
        {
            var issue = await applyIssueTriage.ExecuteAsync(
                id,
                priority,
                request.LabelIds,
                request.AcceptanceCriteria,
                cancellationToken);

            return Ok(ToResponse(issue));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("issues/{id:guid}/assign")]
    public async Task<ActionResult<IssueResponse>> Assign(
        Guid id,
        AssignIssueRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AssigneeUserId == Guid.Empty)
        {
            return ValidationProblem("Assignee user id is required.");
        }

        try
        {
            var issue = await assignIssue.ExecuteAsync(id, request.AssigneeUserId, cancellationToken);
            return Ok(ToResponse(issue));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("issues/{id:guid}/transition")]
    public async Task<ActionResult<IssueResponse>> Transition(
        Guid id,
        TransitionIssueStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IssueStatus>(request.Status, true, out var status))
        {
            return ValidationProblem("Status value is invalid.");
        }

        try
        {
            var issue = await transitionIssueStatus.ExecuteAsync(id, status, cancellationToken);
            return Ok(ToResponse(issue));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private static IssueResponse ToResponse(IssueDto issue)
    {
        return new IssueResponse(
            issue.Id,
            issue.ProjectId,
            issue.Title,
            issue.Description,
            ToStatusResponse(issue.Status),
            issue.Priority.ToString().ToLowerInvariant(),
            issue.ReporterUserId,
            issue.AssigneeUserId,
            issue.AcceptanceCriteria,
            issue.AcceptanceCriteriaIsAiGenerated,
            issue.CreatedAtUtc,
            issue.ClosedAtUtc,
            issue.Labels.Select(label => new IssueLabelResponse(label.Id, label.Name)).ToList());
    }

    private static IssueTriageSuggestionResponse ToSuggestionResponse(IssueTriageSuggestionDto suggestion)
    {
        return new IssueTriageSuggestionResponse(
            suggestion.IssueId,
            suggestion.Priority.ToString().ToLowerInvariant(),
            suggestion.Labels.Select(label => new IssueLabelResponse(label.Id, label.Name)).ToList(),
            suggestion.AcceptanceCriteria,
            suggestion.IsValid,
            suggestion.ValidationError);
    }

    private static string ToStatusResponse(IssueStatus status)
    {
        return status switch
        {
            IssueStatus.InProgress => "in-progress",
            IssueStatus.InReview => "in-review",
            _ => status.ToString().ToLowerInvariant(),
        };
    }

    private static bool TryParseStatus(string? status, out IssueStatus? parsedStatus)
    {
        parsedStatus = null;

        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        parsedStatus = status.Trim().ToLowerInvariant() switch
        {
            "backlog" => IssueStatus.Backlog,
            "todo" => IssueStatus.Todo,
            "in-progress" or "inprogress" => IssueStatus.InProgress,
            "in-review" or "inreview" => IssueStatus.InReview,
            "done" => IssueStatus.Done,
            _ => null,
        };

        return parsedStatus is not null;
    }
}
