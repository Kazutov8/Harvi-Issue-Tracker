using System.ComponentModel.DataAnnotations;

namespace IssueTracker.API.Contracts.Projects;

public sealed record CreateProjectRequest(
    [Required]
    [MinLength(2)]
    string Name);
