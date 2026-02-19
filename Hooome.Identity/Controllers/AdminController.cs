using Hooome.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hooome.Identity.Controllers;

[ApiController]
[Route("admin")]
public class AdminController(
    UserManager<AppUser> userManager) : ControllerBase

{
    [HttpGet("users/count")]
    public async Task<ActionResult<int>> GetUsersCount()
    {
        var count = await userManager.Users.CountAsync();
        
        return Ok(count);
    }
}
