namespace HM.BLL.Models.Users;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string? Email { get; set; }
    public List<ProfileDto> Profiles { get; set; } = [];
    public List<string> Roles { get; set; } = [];
}
