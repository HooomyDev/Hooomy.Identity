using Hooome.Identity.Data;
using Hooome.Identity.Models;
using Hooome.Identity.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hooome.Identity.Controllers;

[ApiController]
[Route("admin")]
public class AdminController(
    UserManager<AppUser> userManager,
    AuthDbContext context) : ControllerBase

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
}
