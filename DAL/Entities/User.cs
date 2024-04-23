using Microsoft.AspNetCore.Identity;

namespace HM.DAL.Entities;

public class User : IdentityUser
{
    public bool IsOidcUser { get; set; }
    public string? OidcToken { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? DeliveryAddress { get; set; }
    public List<Order> Orders { get; set; } = [];
    public long? InvalidateTokenBefore { get; set; }
}
