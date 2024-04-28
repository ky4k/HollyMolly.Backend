using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HM.BLL.UnitTests.TestHelpers;

public static class ServiceHelper
{
    public static DbContextOptions<HmDbContext> GetTestDbContextOptions()
    {
        return new DbContextOptionsBuilder<HmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }
    public static HmDbContext GetTestDbContext()
    {
        return new HmDbContext(GetTestDbContextOptions());
    }

    public static UserManager<User> GetUserManager(HmDbContext context)
    {
        var userStore = new UserStore<User>(context);
        var passwordHasher = new PasswordHasher<User>();
        var options = new OptionsWrapper<IdentityOptions>(GetIdentityOptions());
        var userValidators = new List<IUserValidator<User>>() { new UserValidator<User>() };
        var passwordValidators = new List<IPasswordValidator<User>>() { new PasswordValidator<User>() };
        var lookupNormalizer = new UpperInvariantLookupNormalizer();
        var logger = new Logger<UserManager<User>>(new LoggerFactory());


        var x = new UserManager<User>(userStore,
            options, passwordHasher, userValidators, passwordValidators, lookupNormalizer, null!, null!, logger);
        return Substitute.ForPartsOf<UserManager<User>>(userStore,
            options, passwordHasher, userValidators, passwordValidators, lookupNormalizer, null!, null!, logger);
    }

    public static RoleManager<Role> GetRoleManager(HmDbContext context)
    {
        var roleStore = new RoleStore<Role>(context);
        return Substitute.ForPartsOf<RoleManager<Role>>(roleStore, null!, null!, null!, null!);
    }

    private static IdentityOptions GetIdentityOptions()
    {
        IdentityOptions options = new();
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        return options;
    }
}
