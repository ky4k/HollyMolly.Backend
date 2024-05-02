using HM.DAL.Constants;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.IntegrationTests.TestHelpers;

public static class SeedDefaultEntities
{
    public static async Task SeedAsync(HmDbContext? context, UserManager<User>? userManager, RoleManager<Role>? roleManager)
    {
        if (roleManager != null)
        {
            await roleManager.CreateAsync(new Role(DefaultRoles.Administrator));
            await roleManager.CreateAsync(new Role(DefaultRoles.Manager));
            await roleManager.CreateAsync(new Role(DefaultRoles.User));
        }
        if (userManager != null)
        {
            foreach (User user in Users)
            {
                await userManager.CreateAsync(user);
                await userManager.AddPasswordAsync(user, "password");
                await userManager.AddToRoleAsync(user, DefaultRoles.User);
            }
            foreach (User admin in Admins)
            {
                await userManager.CreateAsync(admin);
                await userManager.AddPasswordAsync(admin, "password");
                await userManager.AddToRoleAsync(admin, DefaultRoles.User);
                await userManager.AddToRoleAsync(admin, DefaultRoles.Administrator);
            }
        }
        if (context != null)
        {
            await context.SaveChangesAsync();
        }
    }
    private static List<User> Users =>
    [
        new()
        {
            Id = "1",
            Email = "user1@example.com",
            UserName = "user1@example.com",
            FirstName = "First",
            LastName = "User"
        },
        new()
        {
            Id = "2",
            UserName = "user2@example.com",
            Email = "user2@example.com",
            FirstName = "Second",
            LastName = "User"
        },
        new()
        {
            Id = "3",
            UserName = "user3@example.com",
            Email = "user3@example.com",
            FirstName = "Third",
            LastName = "User"
        },
        new()
        {
            Id = "4",
            UserName = "user4@example.com",
            Email = "user4@example.com",
            FirstName = "Fourth",
            LastName = "User"
        },
        new()
        {
            Id = "5",
            UserName = "user5@example.com",
            Email = "user5@example.com",
            FirstName = "Fifth",
            LastName = "User"
        },
    ];
    private static List<User> Admins =>
    [
        new()
        {
            Id = "51",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FirstName = "Default",
            LastName = "Administrator"
        }
    ];
}
