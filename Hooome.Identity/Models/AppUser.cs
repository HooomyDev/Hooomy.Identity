using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Hooome.Identity.Models;

public class AppUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string Surname { get; set; } = null!;

    [Required]
    public string Patronymic { get; set; } = "";
}
