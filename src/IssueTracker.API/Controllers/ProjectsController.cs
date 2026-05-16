using System.Security.Claims;
using IssueTracker.API.Contracts.Projects;
using IssueTracker.API.Infrastructure;
using IssueTracker.Application.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.API.Controllers;

[ApiController]
[Authorize]
[Route("projects")]
public sealed class ProjectsController(CreateProject createProject, ListProjects listProjects) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var project = await createProject.ExecuteAsync(request.Name, userId, cancellationToken);
            return Ok(ToResponse(project));
        }
        catch (InvalidOperationException exception)
        {
            return ApiProblemDetailsFactory.Create(this, StatusCodes.Status409Conflict, "Conflict", exception.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> List(CancellationToken cancellationToken)
    {
        var projects = await listProjects.ExecuteAsync(cancellationToken);
        return Ok(projects.Select(ToResponse).ToList());
    }

    private static ProjectResponse ToResponse(ProjectDto project)
    {
        return new ProjectResponse(project.Id, project.Name, project.Slug, project.CreatedByUserId, project.CreatedAtUtc);
    }
}
