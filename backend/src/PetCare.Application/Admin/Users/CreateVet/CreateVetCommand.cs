using Microsoft.AspNetCore.Identity;
using PetCare.Infrastructure.Auth;
using PetCare.Infrastructure.Persistence;
using PetCare.Domain.Users;

namespace PetCare.Application.Admin.Users.CreateVet;

public sealed class CreateVetCommand
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly PetCareDbContext _db;

    public CreateVetCommand(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        PetCareDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<(bool Succeeded, string[] Errors, CreateVetResponse? Data)> 
        ExecuteAsync(CreateVetRequest request, CancellationToken ct = default)
    {
        // 1. Check if email already exists
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            return (false, new[] { "Email is already registered." }, null);
        }

        // 2. Create user object
        var vet = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AccountStatus = AccountStatus.Active
        };

        // 3. Identity handles password hashing internally
        var result = await _userManager.CreateAsync(vet, request.Password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray(), null);
        }

        // 4. Ensure "Vet" role exists
        if (!await _roleManager.RoleExistsAsync("Vet"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Vet"));
        }

        // 5. Assign Vet role
        await _userManager.AddToRoleAsync(vet, "Vet");

        // 6. Return response
        return (true, Array.Empty<string>(), new CreateVetResponse
        {
            Id = vet.Id,
            FullName = vet.FullName,
            Email = vet.Email!,
            Role = "Vet",
            Message = "Vet account created successfully."
        });
    }
}
