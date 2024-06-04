using HM.BLL.Models.Statistics;
using HM.DAL.Entities;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class StatisticsIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public StatisticsIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetProductsStatistics_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Statistics/products");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductStatisticDto>? productStatisticDto = await JsonSerializer.DeserializeAsync<IEnumerable<ProductStatisticDto>>(
            await httpResponse.Content.ReadAsStreamAsync(), jsonSerializerOptions);

        Assert.NotNull(productStatisticDto);
        Assert.NotEmpty(productStatisticDto);
        Assert.Equal(3, productStatisticDto.Count());
    }
    [Fact]
    public async Task GetCategoryGroupsStatistics_ShouldWork()
    {
        int categoryGroupId = 1;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Statistics/categoryGroups/{categoryGroupId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<CategoryStatisticDto>? categoryStatisticDto = await JsonSerializer.DeserializeAsync<IEnumerable<CategoryStatisticDto>>(
            await httpResponse.Content.ReadAsStreamAsync(), jsonSerializerOptions);

        Assert.NotNull(categoryStatisticDto);
        Assert.NotEmpty(categoryStatisticDto);
        Assert.Single(categoryStatisticDto);
    }
    [Fact]
    public async Task GetCategoriesStatistics_ShouldWork()
    {
        int categoryId = 2;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Statistics/categories/{categoryId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<CategoryStatisticDto>? categoryStatisticDto = await JsonSerializer.DeserializeAsync<IEnumerable<CategoryStatisticDto>>(
            await httpResponse.Content.ReadAsStreamAsync(), jsonSerializerOptions);

        Assert.NotNull(categoryStatisticDto);
        Assert.NotEmpty(categoryStatisticDto);
        Assert.Single(categoryStatisticDto);
    }
    [Fact]
    public async Task GetOrdersStatistics_ShouldWork()
    {
        string queryParameters = "includeUnpaid=true";
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Statistics/orders?{queryParameters}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<OrderStatisticDto>? orderStatisticDto = await JsonSerializer.DeserializeAsync<IEnumerable<OrderStatisticDto>>(
            await httpResponse.Content.ReadAsStreamAsync(), jsonSerializerOptions);

        Assert.NotNull(orderStatisticDto);
        Assert.Single(orderStatisticDto);
        Assert.Equal(3, orderStatisticDto.First().NumberOfOrders);
    }
    [Fact]
    public async Task GetEmailLogs_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Statistics/emails");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<EmailLog>? emailStatisticDto = await JsonSerializer.DeserializeAsync<IEnumerable<EmailLog>>(
            await httpResponse.Content.ReadAsStreamAsync(), jsonSerializerOptions);

        Assert.NotNull(emailStatisticDto);
        Assert.NotEmpty(emailStatisticDto);
        Assert.Equal(2, emailStatisticDto.Count());
    }
}
