namespace HM.BLL.Models;

public class RegistrationRequest
{
    private string email = null!;
    public string Email { get => email; set => email = value.ToLower(); }
    public string Password { get; set; } = null!;
}
