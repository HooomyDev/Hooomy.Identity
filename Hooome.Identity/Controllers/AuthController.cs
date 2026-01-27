using Duende.IdentityServer.Services;
using Hooome.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hooome.Identity.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    IIdentityServerInteractionService interactionService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid data" });

        var user = await userManager.FindByEmailAsync(viewModel.Email);
        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var result = await signInManager.PasswordSignInAsync(
            user.UserName,
            viewModel.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var roles = await userManager.GetRolesAsync(user);

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    firstName = user.FirstName,
                    surname = user.Surname,
                    patronymic = user.Patronymic,
                    phoneNumber = user.PhoneNumber,
                    roles
                }
            });
        }

        return Unauthorized(new { message = "Login error" });
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid data" });

        var user = new AppUser
        {
            Email = viewModel.Email,
            UserName = viewModel.Email,
            FirstName = viewModel.FirstName,
            Surname = viewModel.Surname,
            Patronymic = viewModel.Patronymic,
        };

        var result = await userManager.CreateAsync(user, viewModel.Password);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(viewModel.Role))
            {
                await userManager.AddToRoleAsync(user, viewModel.Role);
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { message = "Register successful" });
        }

        return BadRequest(new { message = "Error occurred", errors = result.Errors });
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string? logoutId)
    {
        await signInManager.SignOutAsync();

        if (!string.IsNullOrEmpty(logoutId))
        {
            var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);
            return Ok(new { message = "Logout successful", redirectUri = logoutRequest?.PostLogoutRedirectUri });
        }

        return Ok(new { message = "Logout successful" });
    }
}
