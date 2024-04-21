namespace HM.BLL.Models.Users;

public class LoginRequest
{
    private string email = null!;
    public string Email { get => email; set => email = value.ToLower(); }
    public string Password { get; set; } = null!;
}
