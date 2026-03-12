using Hooome.Identity.Data;
using Hooome.Identity.Models;
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
        var count = await userManager.Users.CountAsync();
        
        return Ok(count);
    }

    [HttpGet("users/list")]
    public async Task<ActionResult<PagedResult<UserWithRoleDto>>> GetUsersPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var usersQuery = userManager.Users
            .OrderBy(u => u.UserName);

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                User = u,
                Roles = (from userRole in context.UserRoles
                         join role in context.Roles on userRole.RoleId equals role.Id
                         where userRole.UserId == u.Id
                         select role.Name).ToList()
            })
            .ToListAsync();

        var userDtos = users.Select(x => new UserWithRoleDto
        {
            Id = x.User.Id,
            FirstName = x.User.FirstName,
            Surname = x.User.Surname,
            Patronymic = x.User.Patronymic,
            Email = x.User.Email,
            PhoneNumber = x.User.PhoneNumber,
            EmailConfirmed = x.User.EmailConfirmed,
            LockoutEnabled = x.User.LockoutEnabled,
            LockoutEnd = x.User.LockoutEnd,
            AccessFailedCount = x.User.AccessFailedCount,
            Roles = x.Roles
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
}

public class UserWithRoleDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string Patronymic { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public List<string> Roles { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}