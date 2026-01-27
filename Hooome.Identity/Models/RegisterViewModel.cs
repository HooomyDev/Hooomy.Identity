using System.ComponentModel.DataAnnotations;

namespace Hooome.Identity.Models;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string Surname { get; set; } = null!;

    [Required]
    public string Patronymic { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;
}
