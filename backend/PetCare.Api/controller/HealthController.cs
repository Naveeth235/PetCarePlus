using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Api.Persistence;

namespace PetCare.Api.controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    public HealthController(AppDbContext db) => _db = db;

    // Simple liveness
    [HttpGet("alive")]
    public IActionResult Alive() => Ok(new { ok = true, time = DateTime.UtcNow });

    // DB connectivity + pending migrations
    [HttpGet("db")]
    public async Task<IActionResult> Db()
    {
        var canConnect = await _db.Database.CanConnectAsync();
        var pending = await _db.Database.GetPendingMigrationsAsync();
        return Ok(new {
            ok = canConnect,
            provider = _db.Database.ProviderName,
            database = _db.Database.GetDbConnection().Database,
            pendingMigrations = pending.ToArray()
        });
    }
}
