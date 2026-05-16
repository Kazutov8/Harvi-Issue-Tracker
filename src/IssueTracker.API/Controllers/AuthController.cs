using System.Security.Claims;
using IssueTracker.API.Contracts.Auth;
using IssueTracker.API.Infrastructure;
using IssueTracker.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.API.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    RegisterUser registerUser,
    LoginUser loginUser,
    GetCurrentUser getCurrentUser,
    ListUsers listUsers) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await registerUser.ExecuteAsync(request.Email, request.Password, request.DisplayName, cancellationToken);
            return Ok(ToResponse(result));
        }
        catch (InvalidOperationException exception)
        {
            return ApiProblemDetailsFactory.Create(this, StatusCodes.Status409Conflict, "Conflict", exception.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await loginUser.ExecuteAsync(request.Email, request.Password, cancellationToken);

        if (result is null)
        {
            return ApiProblemDetailsFactory.Create(this, StatusCodes.Status401Unauthorized, "Unauthorized", "Invalid email or password.");
        }

        return Ok(ToResponse(result));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await getCurrentUser.ExecuteAsync(userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc));
    }

    [Authorize]
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken cancellationToken)
    {
        var users = await listUsers.ExecuteAsync(cancellationToken);

        return Ok(users
            .Select(user => new UserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc))
            .ToList());
    }

    private static AuthResponse ToResponse(AuthResult result)
    {
        return new AuthResponse(
            result.AccessToken,
            new UserResponse(result.User.Id, result.User.Email, result.User.DisplayName, result.User.CreatedAtUtc));
    }
}
