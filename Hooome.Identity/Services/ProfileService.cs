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
        var role = (await userManager.GetRolesAsync(user))[0];

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Email, user.Email),
            new(JwtClaimTypes.FamilyName, user.Surname),
            new(JwtClaimTypes.MiddleName, user.Patronymic),
            new(JwtClaimTypes.GivenName, user.FirstName),
            new(JwtClaimTypes.Name, user.UserName),
            new(JwtClaimTypes.Role, role),
            new(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
        };

        context.IssuedClaims.AddRange(claims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}
