namespace HM.BLL.Models.Users;

public class RegistrationResponse
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
