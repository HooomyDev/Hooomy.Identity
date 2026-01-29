namespace Hooome.Identity.Models;

public class ChangePasswordViewModel
{
    public string Email { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
