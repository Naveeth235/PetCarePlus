using PetCare.Infrastructure.Auth;

namespace PetCare.Infrastructure.Jwt;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAtUtc) Create(ApplicationUser user, string role);
}
