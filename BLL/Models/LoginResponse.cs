namespace HM.BLL.Models;

public class LoginResponse
{
    public string? AccessToken { get; set; }
    public string? UserName { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
