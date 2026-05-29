using Duende.IdentityServer.Services;
using Hooome.Identity.Models;
using Hooome.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hooome.Identity.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    IIdentityServerInteractionService interactionService,
    IEmailService emailService) : ControllerBase
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
            Status = Models.Enums.UserStatus.Pending,
        };

        var result = await userManager.CreateAsync(user, viewModel.Password);

        if (result.Succeeded)
        {
                await userManager.AddToRoleAsync(user, "Resident");

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
        var user = await userManager.FindByEmailAsync(model.Email);
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
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);

            return Ok(new { message = "Password changed successfully" });
        }

        return BadRequest(new { message = "Error changing password", errors = result.Errors });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Не показываем, что пользователь не найден (безопасность)
            return Ok(new { message = "Если пользователь существует, вы получите письмо" });
        }

        // Генерация токена сброса пароля
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Создание ссылки для сброса пароля
        var resetLink = $"http://localhost:3000/reset-password?token={Uri.EscapeDataString(token)}&email={user.Email}";

        // Отправка email
        await emailService.SendPasswordResetEmail(user.Email, user.UserName, resetLink, CancellationToken.None);

        return Ok(new { message = "Инструкция по сбросу пароля отправлена на ваш email" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return BadRequest(new { error = "Недействительный запрос" });
        }

        var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (result.Succeeded)
        {
            // Опционально: отправить уведомление об успешной смене пароля
            await emailService.SendPasswordChangedEmail(user.Email, user.UserName, CancellationToken.None);

            return Ok(new { message = "Пароль успешно изменён" });
        }

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }
}

// DTO для запроса на сброс пароля
public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

// DTO для сброса пароля
public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = null!;
}