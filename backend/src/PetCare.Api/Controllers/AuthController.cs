using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PetCare.Application.Auth.RegisterOwner;

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
}
