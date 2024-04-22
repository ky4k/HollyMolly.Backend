using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HM.WebAPI.Configurations;

public class IdentityConfigureOptions : IConfigureOptions<IdentityOptions>
{
    public void Configure(IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    }
}
