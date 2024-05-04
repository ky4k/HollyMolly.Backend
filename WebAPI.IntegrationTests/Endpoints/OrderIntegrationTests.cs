using HM.BLL.Models.Orders;
using System.Net;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class OrderIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public OrderIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetAllOrders_ShouldReturnOrders()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Orders");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<OrderDto>? orders = JsonSerializer.Deserialize<IEnumerable<OrderDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(orders);
        Assert.NotEmpty(orders);
        Assert.True(orders.Count() >= 3);
    }
    [Fact]
    public async Task GetUserOrders_ShouldReturnOrdersOfTheUser()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Orders/myOrders");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<OrderDto>? orders = JsonSerializer.Deserialize<IEnumerable<OrderDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(orders);
        Assert.Single(orders);
    }
    [Fact]
    public async Task GetOrderById_ShouldReturnOrder()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Orders/1");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(order);
        Assert.Single(order.OrderRecords);
    }
    [Fact]
    public async Task CreateOrder_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Orders");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user1@example.com", "password");
        OrderCreateDto orderCreateDto = new()
        {
            Customer = new CustomerDto()
            {
                Email = "user1@example.com",
                FirstName = "Перше",
                LastName = "Останнє",
                PhoneNumber = "0123456789",
                City = "Місто",
                DeliveryAddress = "Адреса"
            },
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 2,
                    Quantity = 5
                }
            ]
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(orderCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.NotNull(httpResponse.Headers.Location);
        Assert.NotNull(order);
        Assert.Equal("user1@example.com", order.Customer.Email);
        Assert.Equal(2, order.OrderRecords[0].ProductInstanceId);
        Assert.Equal(5, order.OrderRecords[0].Quantity);
    }
    [Fact]
    public async Task CreateOrder_ShouldCreateOrderForSpecifiedUser()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Orders/1");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        OrderCreateDto orderCreateDto = new()
        {
            Customer = new CustomerDto()
            {
                Email = "user1@example.com",
                FirstName = "Перше",
                LastName = "Останнє",
                PhoneNumber = "0123456789",
                City = "Місто",
                DeliveryAddress = "Адреса"
            },
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 2,
                    Quantity = 5
                }
            ]
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(orderCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.NotNull(httpResponse.Headers.Location);
        Assert.NotNull(order);
        Assert.Equal("user1@example.com", order.Customer.Email);
        Assert.Equal(2, order.OrderRecords[0].ProductInstanceId);
        Assert.Equal(5, order.OrderRecords[0].Quantity);
    }
    [Fact]
    public async Task UpdateOrderStatus_ShouldWorks()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Put, "api/Orders/2");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Delivered",
            Notes = "Comment"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(orderUpdateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(order);
        Assert.Equal(orderUpdateDto.Status, order.Status);
        Assert.Equal(orderUpdateDto.Notes, order.Notes);
    }
}
