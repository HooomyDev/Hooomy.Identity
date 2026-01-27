using Microsoft.AspNetCore.Identity;

namespace Hooome.Identity.Data;

public class DbContextInitializer
{
    public static async Task Initialize(AuthDbContext context, 
        RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        string[] roles = ["Resident", "Employee", "Admin"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
