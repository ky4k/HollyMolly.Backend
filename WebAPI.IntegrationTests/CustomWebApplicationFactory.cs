using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.IntegrationTests.TestHelpers;

namespace WebAPI.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public User? AuthenticatedUser { get; set; }
    public FakePolicyEvaluatorOptions? FakePolicyEvaluatorOptions { get; set; }
    public bool UseRealContext { get; set; } = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<HmDbContext>));

            if (contextDescriptor != null)
                services.Remove(contextDescriptor);

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<HmDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDataBase");
                options.UseInternalServiceProvider(serviceProvider);
            });

            services.AddSingleton<IPolicyEvaluator>(new FakePolicyEvaluator(FakePolicyEvaluatorOptions));
        });
    }
}
