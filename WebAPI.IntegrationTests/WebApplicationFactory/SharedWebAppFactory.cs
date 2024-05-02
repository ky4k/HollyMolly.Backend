using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace WebAPI.IntegrationTests.WebApplicationFactory;

public class SharedWebAppFactory : IClassFixture<SharedWebAppFactory>
{
    private WebApplicationFactory<Program>? _factory;
    public WebAppFactoryHelper? FactoryHelper { get; set; }

    public void Initialize()
    {
        if (_factory == null)
        {
            FactoryHelper ??= new WebAppFactoryHelper();
            _factory = FactoryHelper.CreateWebApplicationFactory();
        }
    }
    public void Initialize(WebAppFactoryHelper factoryHelper)
    {
        FactoryHelper = factoryHelper;
        Initialize();
    }

    public HttpClient CreateClient()
    {
        if (_factory == null)
        {
            Initialize();
        }
        return _factory!.CreateClient();
    }
    public void Dispose()
    {
        _factory?.Dispose();
    }

    public async Task SeedContextAsync(Func<HmDbContext?, UserManager<User>?, RoleManager<Role>?, Task> seedFunction)
    {
        if (_factory == null)
        {
            Initialize();
        }
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
        await seedFunction(context, userManager, roleManager);
        await context!.SaveChangesAsync();
    }
}
