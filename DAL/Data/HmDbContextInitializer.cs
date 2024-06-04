using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.DAL.Data;

public class HmDbContextInitializer(
    HmDbContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ILogger<HmDbContextInitializer> logger
    )
{
    public async Task ApplyMigrationsAsync()
    {
        try
        {
            if (!context.Database.IsRelational())
            {
                logger.LogInformation("Migrations cannot be applied to a non relational database.");
            }

            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                logger.LogInformation("Apply migrations.");
                await context.Database.MigrateAsync();
                logger.LogInformation("All migrations was successfully applied.");
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

    public async Task SeedAsync(string? adminEmail, string? adminPassword)
    {
        try
        {
            await SeedDefaultRolesAsync();
            await SeedDefaultAdminAsync(adminEmail, adminPassword);
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
            DefaultRoles.User
        ];
        List<string> existing = await roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToListAsync();
        IEnumerable<string> rolesToAdd = roles.Except(existing);
        if (rolesToAdd.Any())
        {
            foreach (string role in rolesToAdd)
            {
                await roleManager.CreateAsync(new Role(role));
                logger.LogInformation("Seeding the role {Role}", role);
            }
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedDefaultAdminAsync(string? adminEmail, string? adminPassword)
    {
        if (string.IsNullOrEmpty(adminEmail)
            || string.IsNullOrEmpty(adminPassword))
        {
            logger.LogInformation("Default admin information was not provided. " +
                "Default admin was not seeded.");
            return;
        }
        if (await userManager.FindByEmailAsync(adminEmail) != null)
        {
            logger.LogInformation("User with the email '{AdminEmail}' already exists.", adminEmail);
            return;
        }

        var administrator = new User { UserName = adminEmail, Email = adminEmail };
        await userManager.CreateAsync(administrator, adminPassword);
        await userManager.AddToRoleAsync(administrator, DefaultRoles.Administrator);
        logger.LogInformation("Default admin '{AdminEmail}' has been seeded.", adminEmail);
    }
}
