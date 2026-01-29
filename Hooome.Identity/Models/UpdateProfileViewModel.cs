using System.ComponentModel.DataAnnotations;

namespace Hooome.Identity.Models;

public class UpdateProfileViewModel
{
    [Required]
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Patronymic { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}
