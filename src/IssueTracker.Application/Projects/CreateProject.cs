using System.Text;
using IssueTracker.Application.Abstractions;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Application.Projects;

public sealed class CreateProject(IProjectRepository projectRepository, IApplicationDbContext applicationDbContext)
{
    public async Task<ProjectDto> ExecuteAsync(string name, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var slug = CreateSlug(normalizedName);
        var existingProject = await projectRepository.GetBySlugAsync(slug, cancellationToken);

        if (existingProject is not null)
        {
            throw new InvalidOperationException("A project with this slug already exists.");
        }

        var project = Project.Create(normalizedName, slug, createdByUserId);

        await projectRepository.AddAsync(project, cancellationToken);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Slug, project.CreatedByUserId, project.CreatedAtUtc);
    }

    private static string CreateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        var builder = new StringBuilder();
        var previousWasDash = false;

        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasDash = false;
            }
            else if (!previousWasDash)
            {
                builder.Append('-');
                previousWasDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException("Project name must contain at least one letter or number.");
        }

        return slug;
    }
}
