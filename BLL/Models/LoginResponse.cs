namespace HM.BLL.Models;

public class LoginResponse
{
    public string? AccessToken { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
