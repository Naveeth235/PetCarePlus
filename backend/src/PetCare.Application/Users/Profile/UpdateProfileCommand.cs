using Microsoft.AspNetCore.Identity;              // UserManager
using PetCare.Infrastructure.Auth;                // ApplicationUser

namespace PetCare.Application.Users.Profile;

public sealed class UpdateProfileCommand
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateProfileCommand(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // userId comes from JWT (ClaimTypes.NameIdentifier) in the controller
    public async Task<(bool ok, string? error, UpdateProfileResponse? data)> ExecuteAsync(
        string userId,
        UpdateProfileRequest req,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "not_found", null);

        // Allowed updates only
        user.FullName = req.FullName;
        user.PhoneNumber = string.IsNullOrWhiteSpace(req.PhoneNumber) ? null : req.PhoneNumber;

        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
        {
            var why = string.Join(", ", res.Errors.Select(e => e.Description));
            return (false, $"identity_error: {why}", null);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return (true, null, new UpdateProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? "Owner"
        });
    }
}
