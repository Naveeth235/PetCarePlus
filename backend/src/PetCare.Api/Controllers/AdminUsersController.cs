using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.Admin.Users.CreateVet;
using PetCare.Infrastructure.Persistence; 

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    // ---------- CREATE VET ----------
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

    // ---------- LIST VETS (paging + search) ----------
    // GET /api/admin/users/vets?search=&page=1&pageSize=10
    [HttpGet("vets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListVets(
        [FromServices] PetCareDbContext db,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // input safety
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        // users with VET role (use NormalizedName for casing-robust match)
        var query =
            from u in db.Users
            join ur in db.UserRoles on u.Id equals ur.UserId
            join r in db.Roles on ur.RoleId equals r.Id
            where r.NormalizedName == "VET"
            select new VetListItem
            {
                Id = u.Id,
                FullName = u.FullName,  // adjust if your ApplicationUser uses a different property name
                Email = u.Email!
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                (x.FullName ?? "").ToLower().Contains(s) ||
                (x.Email ?? "").ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new VetListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // Local DTOs (keep it here for now; can move to Application later)
    private sealed class VetListItem
    {
        public string Id { get; set; } = default!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    private sealed class VetListResponse
    {
        public List<VetListItem> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
