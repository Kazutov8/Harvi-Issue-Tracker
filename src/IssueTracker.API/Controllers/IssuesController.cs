using System.Security.Claims;
using IssueTracker.API.Contracts.Issues;
using IssueTracker.API.Contracts.Labels;
using IssueTracker.Application.Issues;
using IssueTracker.Application.Labels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.API.Controllers;

[ApiController]
[Authorize]
public sealed class IssuesController(
    CreateIssue createIssue,
    GetIssueDetails getIssueDetails,
    ListProjectIssues listProjectIssues,
    ListProjectLabels listProjectLabels) : ControllerBase
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
    public async Task<ActionResult<IReadOnlyList<IssueResponse>>> List(string projectSlug, CancellationToken cancellationToken)
    {
        try
        {
            var issues = await listProjectIssues.ExecuteAsync(projectSlug, cancellationToken);
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

    private static IssueResponse ToResponse(IssueDto issue)
    {
        return new IssueResponse(
            issue.Id,
            issue.ProjectId,
            issue.Title,
            issue.Description,
            issue.Status.ToString().ToLowerInvariant(),
            issue.Priority.ToString().ToLowerInvariant(),
            issue.ReporterUserId,
            issue.AssigneeUserId,
            issue.AcceptanceCriteria,
            issue.AcceptanceCriteriaIsAiGenerated,
            issue.CreatedAtUtc,
            issue.ClosedAtUtc,
            issue.Labels.Select(label => new IssueLabelResponse(label.Id, label.Name)).ToList());
    }
}
