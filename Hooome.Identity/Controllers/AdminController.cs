using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Hooome.Identity.Models.Enums;
using Hooome.Identity.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Hooome.Identity.Controllers;

[ApiController]
[Route("admin")]
public class AdminController(
    UserManager<AppUser> userManager,
    IPassGenService passGenService,
    IEmailService emailService) : ControllerBase
{
    [HttpGet("users/count")]
    public async Task<ActionResult<int>> GetUsersCount()
    {
        var users = await userManager.Users.ToListAsync();
        var count = 0;

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
                count++;
        }

        return Ok(count);
    }

    [HttpGet("users/list")]
    public async Task<ActionResult<PagedResult<UserWithRoleDto>>> GetUsersPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        // Получаем всех пользователей
        var allUsers = await userManager.Users.ToListAsync();

        var nonAdminUsers = new List<AppUser>();
        var rolesCache = new Dictionary<string, IList<string>>();

        foreach (var user in allUsers)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
            {
                nonAdminUsers.Add(user);
                rolesCache[user.Id] = roles;
            }
        }

        var totalCount = nonAdminUsers.Count;
        var pagedUsers = nonAdminUsers
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userDtos = pagedUsers.Select(user => new UserWithRoleDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            Surname = user.Surname,
            Patronymic = user.Patronymic,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            Roles = rolesCache[user.Id].ToList(),
            Status = user.Status.ToString()
        }).ToList();

        var result = new PagedResult<UserWithRoleDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(result);
    }

    [HttpPut("users/{id:guid}/status")]
    public async Task<ActionResult> UpdateUserStatus(Guid id, [FromQuery] string status)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
            return NotFound(new { error = $"User with id {id} not found" });

        if (!Enum.TryParse<UserStatus>(status, true, out var userStatus))
        {
            return BadRequest(new { error = $"Invalid status. Allowed values: {string.Join(", ", Enum.GetNames<UserStatus>())}" });
        }

        user.Status = userStatus;
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(new { errors = result.Errors });
    }

    [HttpGet("users/get-users-for-company/{companyId:guid}")]
    public async Task<ActionResult<List<UserWithRoleDto>>> GetUsersForCompany(Guid companyId)
    {
        var users = await userManager.Users
            .Where(u => u.CompanyId == companyId.ToString())
            .ToListAsync();

        var companyUsers = new List<UserWithRoleDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains("Employee"))
            {
                companyUsers.Add(new UserWithRoleDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.Surname,
                    Patronymic = user.Patronymic,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = [.. roles],
                    Status = user.Status.ToString(),
                });
            }
        }

        return Ok(companyUsers);
    }

    [HttpPost("users/add-to-company")]
    public async Task<ActionResult> AddToCompany([FromBody] AddUserToCompanyDto dto)
    {
        if (dto == null)
            return BadRequest(new { error = "User data is required" });

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { error = "Email is required" });

        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { error = $"User with email {dto.Email} already exists" });

        var password = passGenService.Generate();

        var user = new AppUser()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            Surname = dto.Surname,
            Patronymic = dto.Patronymic,
            CompanyId = dto.CompanyId.ToString(),
            Status = UserStatus.Approved,
            EmailConfirmed = true,
            LockoutEnabled = false,
            MustChangePassword = true,
        };

        var result = await userManager.CreateAsync(user, password);

        var role = "Employee";

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }
        }

        await emailService.SendPasswordEmail(
            toEmail: user.Email,
            firstName: user.FirstName,
            surname: user.Surname,
            password: password
        );

        return Ok(new
        {
            Name = user.UserName,
            Password = password,
        });
    }

    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return NotFound(new { error = "User not found" });

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains("Employee"))
            return Forbid();

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }
}
