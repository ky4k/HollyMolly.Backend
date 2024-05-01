using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.IntegrationTests.TestHelpers;

public static class SeedDefaultEntities
{
    public static async Task SeedAsync(HmDbContext context)
    {
        await context.Roles.AddRangeAsync(Roles);
        await context.Users.AddRangeAsync(Users);
        await context.UserRoles.AddRangeAsync(UserRoles);
        await context.SaveChangesAsync();
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
            PasswordHash = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
            FirstName = "First",
            LastName = "User"
        },
        new()
        {
            Id = "2",
            UserName = "user2@example.com",
            Email = "user2@example.com",
            PasswordHash = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
            FirstName = "Second",
            LastName = "User"
        },
        new()
        {
            Id = "3",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            PasswordHash = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
            FirstName = "Default",
            LastName = "Administrator"
        }
    ];

    private static List<IdentityUserRole<string>> UserRoles =>
    [
        new()
        {
            RoleId = "3",
            UserId = "1"
        },
        new()
        {
            RoleId = "3",
            UserId = "2"
        },
        new()
        {
            RoleId = "1",
            UserId = "3"
        },
        new()
        {
            RoleId = "3",
            UserId = "3"
        }
    ];
}
