using Microsoft.AspNetCore.Identity;

namespace HM.DAL.Entities;

public class Role : IdentityRole
{
    public Role() : base()
    { }

    public Role(string roleName) : base(roleName)
    { }
}
