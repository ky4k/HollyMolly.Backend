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
            foreach(var role in Roles)
            {
                await roleManager.CreateAsync(role);
            }
        }
        if (userManager != null)
        {
            foreach (User user in Users)
            {
                await userManager.CreateAsync(user);
                await userManager.AddPasswordAsync(user, "password");
                await userManager.AddToRoleAsync(user, DefaultRoles.User);
            }
        }
        if (context != null)
        {
            await context.SaveChangesAsync();
        }
    }

    private static List<Role> Roles =>
    [
        new()
        {
            Id = "1",
            Name = "Administrator",
        },
        new()
        {
            Id = "2",
            Name = "Manager",
        },
        new()
        {
            Id = "3",
            Name = "Registered user"
        }
    ];
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
            Id = "51",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FirstName = "Default",
            LastName = "Administrator"
        }
    ];
}
