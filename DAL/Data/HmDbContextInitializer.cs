using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HM.DAL.Data;

public class HmDbContextInitializer(
    IConfiguration configuration,
    ILogger<HmDbContextInitializer> logger,
    HmDbContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager
    )
{
    public async Task ApplyMigrationsAsync()
    {
        try
        {
            if (!context.Database.IsRelational())
            {
                logger.LogInformation("Migrations cannot be applied to a non relational database");
            }

            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Apply migrations.");
                await context.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("No new migration to apply.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedDefaultRolesAsync();
            await SeedDefaultAdminAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private async Task SeedDefaultRolesAsync()
    {
        List<string> roles = [
            DefaultRoles.Administrator,
            DefaultRoles.Manager,
            DefaultRoles.Consultant,
            DefaultRoles.User
        ];
        List<string> existing = await roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToListAsync();

        foreach (string role in roles.Except(existing))
        {
            await roleManager.CreateAsync(new Role(role));
            logger.LogInformation("Seeding the role {role}", role);
        }
        await context.SaveChangesAsync();
    }

    private async Task SeedDefaultAdminAsync()
    {
        string? adminName = GetConfigurationValue("DefaultAdmin:Name");
        string? adminEmail = GetConfigurationValue("DefaultAdmin:Email");
        string? adminPassword = GetConfigurationValue("DefaultAdmin:Password");
        if (string.IsNullOrEmpty(adminName)
            || string.IsNullOrEmpty(adminEmail)
            || string.IsNullOrEmpty(adminPassword))
        {
            logger.LogInformation("Default admin information was not provided. " +
                "Default admin was not seeded.");
            return;
        }
        if (await userManager.Users.AnyAsync(u => u.UserName == adminName))
        {
            logger.LogInformation("User with the name '{adminName}' already exists.", adminName);
            return;
        }

        var administrator = new User { UserName = adminName, Email = adminEmail };
        await userManager.CreateAsync(administrator, adminPassword);
        await userManager.AddToRoleAsync(administrator, DefaultRoles.Administrator);
        await context.SaveChangesAsync();
        logger.LogInformation("Default admin '{adminName}' has been seeded.", adminName);
    }

    private string? GetConfigurationValue(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
        {
            value = configuration[key];
        }
        return value;
    }
}
