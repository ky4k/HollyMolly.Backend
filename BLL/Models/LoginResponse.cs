namespace BLL.Models;

public class LoginResponse
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? UserName { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
