using HM.BLL.Interfaces;
using HM.DAL.Data;
using HM.DAL.Entities;
using HM.WebAPI.Middleware;
using Microsoft.AspNetCore.Identity;

namespace HM.WebAPI.Extensions;

public static class ApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication application)
    {
        using var scope = application.Services.CreateScope();
        var cfgHelper = scope.ServiceProvider.GetRequiredService<IConfigurationHelper>();
        var context = scope.ServiceProvider.GetRequiredService<HmDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HmDbContextInitializer>>();
        var initializer = new HmDbContextInitializer(context, userManager, roleManager, logger);
        string? adminEmail = cfgHelper.GetConfigurationValue("DefaultAdmin:Email");
        string? adminPassword = cfgHelper.GetConfigurationValue("DefaultAdmin:Password");

        await initializer.ApplyMigrationsAsync();
        await initializer.SeedAsync(adminEmail, adminPassword);
    }
    public static void UseReplaceAuthorizationHeader(this WebApplication application)
    {
        application.UseMiddleware<ReplaceAuthorizationHeaderMiddleware>();
    }
    public static void UseTokenRevocation(this WebApplication application)
    {
        application.UseMiddleware<TokenRevocationMiddleware>();
    }
}
