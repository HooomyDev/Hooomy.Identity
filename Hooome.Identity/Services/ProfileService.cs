using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Hooome.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Hooome.Identity.Services;

public class ProfileService(UserManager<AppUser> userManager) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Resident";

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtClaimTypes.FamilyName, user.Surname ?? string.Empty),
            new(JwtClaimTypes.MiddleName, user.Patronymic ?? string.Empty),
            new(JwtClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(JwtClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtClaimTypes.Role, role),
            new(JwtClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty),
        };

        context.IssuedClaims.AddRange(claims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}
