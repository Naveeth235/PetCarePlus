using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Admin.Users.CreateVet;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]   // only Admins can call these
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
        var (ok, errors, data) = await handler.ExecuteAsync(request, ct);

        if (!ok)
        {
            if (errors.Any(e => e.Contains("already registered", StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Email already in use",
                    Detail = errors.First(),
                    Status = StatusCodes.Status409Conflict
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Vet creation failed",
                Detail = string.Join("; ", errors),
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Created(string.Empty, data);
    }
}
