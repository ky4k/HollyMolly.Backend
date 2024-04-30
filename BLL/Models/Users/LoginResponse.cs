namespace HM.BLL.Models.Users;

public class LoginResponse
{
    public string UserId { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public List<string> Roles { get; set; } = [];
}
