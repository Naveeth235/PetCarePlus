using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Admin.Users.CreateVet;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    [HttpPost("vets")]
    [ProducesResponseType(typeof(CreateVetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVet(
        [FromBody] CreateVetRequest request,
        [FromServices] CreateVetCommand handler,
        CancellationToken ct)
    {
        var (ok, error, data) = await handler.ExecuteAsync(request, ct);

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
                "validation_failed" => BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Detail = "Missing or invalid fields.",
                    Status = StatusCodes.Status400BadRequest
                }),
                _ when error?.StartsWith("identity_error") == true
                    => BadRequest(new ProblemDetails { Title = "Identity error", Detail = error, Status = 400 }),
                _ when error?.StartsWith("role_error") == true
                    => BadRequest(new ProblemDetails { Title = "Role assignment failed", Detail = error, Status = 400 }),
                _ => BadRequest(new ProblemDetails { Title = "Create vet failed", Detail = error ?? "Unknown error", Status = 400 })
            };
        }

        return Created(string.Empty, data);
    }
}
