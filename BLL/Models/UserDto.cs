using HM.BLL.Interfaces;

namespace HM.BLL.Models;

public class UserDto : IUserMailInfo
{
    public string Id { get; set; } = null!;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? DeliveryAddress { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}
