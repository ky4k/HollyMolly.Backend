using Microsoft.AspNetCore.Identity;

namespace HM.DAL.Entities;

public class User : IdentityUser
{
    public bool IsOidcUser { get; set; }
    public string? OidcToken { get; set; }
    public List<Profile> Profiles { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public long? InvalidateTokenBefore { get; set; }
}
