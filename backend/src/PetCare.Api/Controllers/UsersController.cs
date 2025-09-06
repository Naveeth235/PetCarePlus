using System.Security.Claims;                          // ClaimTypes
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Users.Profile;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [Authorize] // any authenticated user (Owner/Vet/Admin)
    [HttpPut("me")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest request,
        [FromServices] UpdateProfileCommand handler,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var (ok, error, data) = await handler.ExecuteAsync(userId, request, ct);

        if (!ok)
        {
            return error switch
            {
                "not_found" => Unauthorized(),
                _ when error?.StartsWith("identity_error") == true
                    => BadRequest(new ProblemDetails { Title = "Identity error", Detail = error, Status = 400 }),
                _ => BadRequest(new ProblemDetails { Title = "Update failed", Detail = error ?? "Unknown error", Status = 400 })
            };
        }

        return Ok(data);
    }
}
