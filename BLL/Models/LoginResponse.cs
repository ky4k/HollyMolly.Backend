namespace HM.BLL.Models;

public class LoginResponse
{
    public string UserId { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string? AccessToken { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
