using Microsoft.AspNetCore.Identity;
using PetCare.Domain.Users;

namespace PetCare.Infrastructure.Auth;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = default!;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
}
