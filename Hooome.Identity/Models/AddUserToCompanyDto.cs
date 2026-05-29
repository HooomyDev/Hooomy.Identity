namespace Hooome.Identity.Models;

public class AddUserToCompanyDto
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Patronymic { get; set; } = "";
    public Guid CompanyId { get; set; }
}