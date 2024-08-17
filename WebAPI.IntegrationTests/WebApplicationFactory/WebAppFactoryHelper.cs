using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.DAL.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.IntegrationTests.Mocks;

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

                var newPostDescriptor = services.FirstOrDefault(
                    d => d.ServiceType == typeof(INewPostCityesService));
                if (newPostDescriptor != null)
                {
                    services.Remove(newPostDescriptor);
                }
                services.AddScoped<INewPostCityesService, MockNewPostService>();

                var emailSenderDescriptor = services.FirstOrDefault(
                    d => d.ServiceType == typeof(IEmailSender));
                if (emailSenderDescriptor != null)
                {
                    services.Remove(emailSenderDescriptor);
                }
                services.AddScoped<IEmailSender, MockEmailSender>();
            })
        );
        return factory;
    }
}
