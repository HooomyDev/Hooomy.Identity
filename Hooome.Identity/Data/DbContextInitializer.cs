using Hooome.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Hooome.Identity.Data;

public class DbContextInitializer
{
    public static async Task Initialize(AuthDbContext context,
        UserManager<AppUser> userManager,
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

        await InitializeUsers(userManager);
    }

    private static async Task InitializeUsers(
        UserManager<AppUser> userManager)
    {
        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var firstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME");
        var surname = Environment.GetEnvironmentVariable("ADMIN_SURNAME");
        var patronymic = Environment.GetEnvironmentVariable("ADMIN_PATRONYMIC");
        var role = "Admin";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("User not created");
            return;
        }

        var existing = await userManager.FindByEmailAsync(email);

        if (existing is null)
        {
            var newUser = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName ?? string.Empty,
                Surname = surname ?? string.Empty,
                Patronymic = patronymic ?? string.Empty,
            };

            var result = await userManager.CreateAsync(newUser, password ?? "Default@123456");

            if (result.Succeeded && !string.IsNullOrEmpty(role))
            {
                await userManager.AddToRoleAsync(newUser, role);
            }
        }
    }
}
