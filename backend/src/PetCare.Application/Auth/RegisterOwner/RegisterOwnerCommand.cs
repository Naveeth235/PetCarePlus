using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Pets;
using PetCare.Domain.Users;
using PetCare.Infrastructure.Auth;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Application.Auth.RegisterOwner;

public sealed class RegisterOwnerCommand
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly PetCareDbContext _db;

    public RegisterOwnerCommand(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        PetCareDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<(bool ok, string? error)> ExecuteAsync(RegisterOwnerRequest request, CancellationToken ct = default)
    {
        // 1) Email unique
        var exists = await _userManager.Users.AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, ct);
        if (exists)
            return (false, "email_in_use");

        // 2) Create user (AccountStatus = Active)
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AccountStatus = AccountStatus.Active
        };

        // ❗ Hashing happens INSIDE ASP.NET Identity:
        // UserManager<ApplicationUser>.CreateAsync(user, password)
        // → uses Microsoft.AspNetCore.Identity's built-in PasswordHasher to hash & store 'PasswordHash'
        var createRes = await _userManager.CreateAsync(user, request.Password);
        if (!createRes.Succeeded)
        {
            var reason = string.Join(", ", createRes.Errors.Select(e => e.Description));
            return (false, $"identity_error: {reason}");
        }

        // 3) Ensure Owner role exists and assign it
        if (!await _roleManager.RoleExistsAsync("Owner"))
            await _roleManager.CreateAsync(new IdentityRole("Owner"));

        var roleRes = await _userManager.AddToRoleAsync(user, "Owner");
        if (!roleRes.Succeeded)
        {
            var reason = string.Join(", ", roleRes.Errors.Select(e => e.Description));
            return (false, $"role_error: {reason}");
        }

        // 4) Optional 0–1 Pet
        if (request.Pet is not null)
        {
            var pet = new Pet
            {
                OwnerUserId = user.Id,
                Name = request.Pet.Name,
                Species = request.Pet.Species,
                Breed = request.Pet.Breed,
                Dob = request.Pet.Dob,
                IsActive = true
            };
            _db.Pets.Add(pet);
        }

        await _db.SaveChangesAsync(ct);
        return (true, null);
    }
}
