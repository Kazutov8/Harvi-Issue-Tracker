using IssueTracker.API.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.API.Infrastructure;

public static class ApiProblemDetailsFactory
{
    public static ObjectResult Create(
        ControllerBase controller,
        int statusCode,
        string title,
        string detail)
    {
        return controller.StatusCode(
            statusCode,
            new ApiErrorResponse(statusCode, title, detail));
    }

    public static BadRequestObjectResult CreateValidation(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The input was invalid." : error.ErrorMessage)
                    .ToArray(),
                StringComparer.Ordinal);

        return new BadRequestObjectResult(new ApiErrorResponse(StatusCodes.Status400BadRequest, "Validation failed.", null, errors));
    }
}
