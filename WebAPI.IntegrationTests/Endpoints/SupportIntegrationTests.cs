using HM.BLL.Models.Supports;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class SupportIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public SupportIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _httpClient = factory.CreateClient();
    }
    [Fact]
    public async Task CreateSupportRequest_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Support");
        SupportCreateDto supportCreateDto = new()
        {
            Name = "Користувач",
            Email = "user1@example.com",
            Description = "Problem",
            Topic = 0
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(supportCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Support? support = await context!.Supports
            .FirstOrDefaultAsync(s => s.Email == supportCreateDto.Email);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.NotNull(support);
        Assert.Equal(supportCreateDto.Name, support.Name);
        Assert.Equal(supportCreateDto.Description, support.Description);
        Assert.Equal(supportCreateDto.Topic, support.Topic);
    }
}
