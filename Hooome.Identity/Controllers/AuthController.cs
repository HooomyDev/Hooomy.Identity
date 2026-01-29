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

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id.ToString());
        if (user == null) return NotFound("User not found");

        user.FirstName = model.FirstName;
        user.Surname = model.Surname;
        user.Patronymic = model.Patronymic;
        user.PhoneNumber = model.PhoneNumber;

        if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
        {
            var setEmailResult = await userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
                return BadRequest(setEmailResult.Errors);

            var setUserNameResult = await userManager.SetUserNameAsync(user, model.Email);
            if (!setUserNameResult.Succeeded)
                return BadRequest(setUserNameResult.Errors);
        }

        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            var roles = await userManager.GetRolesAsync(user);

            return Ok(new
            {
                message = "Profile updated",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    surname = user.Surname,
                    patronymic = user.Patronymic,
                    phoneNumber = user.PhoneNumber,
                    role = roles[0]
                }
            });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null) return NotFound("User not found");

        var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok(new { message = "Password changed successfully" });
        }

        return BadRequest(new { message = "Error changing password", errors = result.Errors });
    }
}
