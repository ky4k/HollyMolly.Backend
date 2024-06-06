using HM.BLL.Models.NewsSubscriptions;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class NewsSubscriptionsIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public NewsSubscriptionsIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetAllSubscriptions_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/NewsSubscriptions");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using Stream stream = await httpResponse.Content.ReadAsStreamAsync();
        IEnumerable<NewsSubscriptionDto>? newsSubscriptions = await JsonSerializer.DeserializeAsync<IEnumerable<NewsSubscriptionDto>>(
            stream, jsonSerializerOptions);

        Assert.NotNull(newsSubscriptions);
        Assert.NotEmpty(newsSubscriptions);
        Assert.Equal(2, newsSubscriptions.Count());
    }
    [Fact]
    public async Task AddSubscription_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/NewsSubscriptions");
        NewsSubscriptionCreateDto newsSubscriptionCreateDto = new()
        {
            Email = "test@example.com"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(newsSubscriptionCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        NewsSubscription? newsSubscription = await context!.NewsSubscriptions
            .FirstOrDefaultAsync(ns => ns.Email == "test@example.com");

        Assert.NotNull(newsSubscription);
    }
    [Fact]
    public async Task CancelSubscription_ShouldWork()
    {
        string removeToken = "remove1";
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/NewsSubscriptions/{removeToken}");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        NewsSubscription? newsSubscription = await context!.NewsSubscriptions
            .FirstOrDefaultAsync(ns => ns.Id == 1);

        Assert.Null(newsSubscription);
    }
}
