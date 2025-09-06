using Microsoft.AspNetCore.Identity;              // UserManager, role APIs
using PetCare.Domain.Users;                       // AccountStatus
using PetCare.Infrastructure.Auth;                // ApplicationUser
using PetCare.Infrastructure.Jwt;                 // IJwtTokenGenerator

namespace PetCare.Application.Auth.Login;

public sealed class LoginQuery
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokens;

    public LoginQuery(UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokens)
    {
        _userManager = userManager;
        _tokens = tokens;
    }

    public async Task<(bool ok, string? error, LoginResponse? data)> ExecuteAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        // 1) Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return (false, "invalid_credentials", null);

        // 2) Check account status
        if (user.AccountStatus != AccountStatus.Active)
            return (false, "inactive", null);

        // 3) Verify password
        //    ASP.NET Identity handles hashing internally when comparing.
        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return (false, "invalid_credentials", null);

        // 4) Role & token
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Owner";

        var (token, expiresAtUtc) = _tokens.Create(user, role);

        var response = new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAtUtc,
            User = new LoginResponse.UserDto
            {
                Id = user.Id,
                Role = role,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty
            }
        };

        return (true, null, response);
    }
}
