using Microsoft.AspNetCore.Identity;
using PetCare.Infrastructure.Auth;

namespace PetCare.Application.Admin.Users.CreateVet;

public sealed class CreateVetCommand
{
    private readonly UserManager<ApplicationUser> _users;

    public CreateVetCommand(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    // returns (ok, error, data)
    // errors: "validation_failed" | "email_in_use" | "identity_error:*" | "role_error:*"
    public async Task<(bool ok, string? error, CreateVetResponse? data)> ExecuteAsync(
        CreateVetRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant();
        var fullName = request.FullName?.Trim();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return (false, "validation_failed", null);
        }

        var existing = await _users.FindByEmailAsync(email);
        if (existing is not null)
        {
            return (false, "email_in_use", null);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var create = await _users.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
            return (false, $"identity_error:{msg}", null);
        }

        var roleAdd = await _users.AddToRoleAsync(user, "VET");
        if (!roleAdd.Succeeded)
        {
            var msg = string.Join("; ", roleAdd.Errors.Select(e => $"{e.Code}:{e.Description}"));
            return (false, $"role_error:{msg}", null);
        }

        return (true, null, new CreateVetResponse(user.Id, email));
    }
}
