using HM.DAL.Data;
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

    public async Task SeedContextAsync(Func<HmDbContext, Task> seedFunction)
    {
        if (_factory == null)
        {
            Initialize();
        }
        using var test = _factory!.Services.CreateScope();
        var context = test.ServiceProvider.GetService<HmDbContext>();
        await seedFunction(context!);
        await context!.SaveChangesAsync();
    }
}
