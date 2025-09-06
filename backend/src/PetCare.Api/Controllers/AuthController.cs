using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PetCare.Application.Auth.RegisterOwner;
using PetCare.Application.Auth.Login;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register-owner")]
    [ProducesResponseType(typeof(RegisterOwnerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterOwner(
        [FromBody] RegisterOwnerRequest request,
        [FromServices] RegisterOwnerCommand handler,
        CancellationToken ct)
    {
        var (ok, error) = await handler.ExecuteAsync(request, ct);

        if (!ok)
        {
            return error switch
            {
                "email_in_use" => Conflict(new ProblemDetails
                {
                    Title = "Email already in use",
                    Detail = "An account with this email already exists.",
                    Status = StatusCodes.Status409Conflict
                }),
                _ when error?.StartsWith("identity_error") == true
                    => BadRequest(new ProblemDetails { Title = "Identity error", Detail = error, Status = 400 }),
                _ when error?.StartsWith("role_error") == true
                    => BadRequest(new ProblemDetails { Title = "Role assignment failed", Detail = error, Status = 400 }),
                _ => BadRequest(new ProblemDetails { Title = "Registration failed", Detail = error ?? "Unknown error", Status = 400 })
            };
        }

        return Created(string.Empty, new RegisterOwnerResponse());
    }
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginQuery handler,
        CancellationToken ct)
    {
        var (ok, error, data) = await handler.ExecuteAsync(request, ct);

        if (!ok)
        {
            return error switch
            {
                "invalid_credentials" => Unauthorized(new ProblemDetails
                {
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized
                }),
                "inactive" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Title = "Account inactive",
                    Detail = "This account is not active. Contact support.",
                    Status = StatusCodes.Status403Forbidden
                }),
                _ => Unauthorized()
            };
        }

        return Ok(data);
    }
}
