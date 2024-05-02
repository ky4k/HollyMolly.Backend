using HM.DAL.Data;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebAPI.IntegrationTests.WebApplicationFactory;

public class WebAppFactoryHelper
{
    internal WebApplicationFactory<Program> CreateWebApplicationFactory()
    {
        WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
            {
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<HmDbContext>));

                if (contextDescriptor != null)
                {
                    services.Remove(contextDescriptor);
                }
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<HmDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDataBase");
                    options.UseInternalServiceProvider(serviceProvider);
                });
            })
        );
        return factory;
    }
}
