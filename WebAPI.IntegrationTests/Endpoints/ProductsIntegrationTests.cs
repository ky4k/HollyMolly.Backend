using HM.BLL.Models.Products;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class ProductsIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public ProductsIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetProductsByCategory_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Products");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductDto>? products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.True(products.Count() >= 8);
    }
    [Fact]
    public async Task GetRecommendedProducts_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Products/recommended?number=5");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductDto>? products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Equal(5, products.Count());
    }
    [Fact]
    public async Task GetProductById_ShouldReturnProduct()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Products/1");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        ProductDto? product = JsonSerializer.Deserialize<ProductDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(product);
        Assert.Equal("Product 1", product.Name);
    }
    [Fact]
    public async Task CreateProduct_ShouldWork()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Products");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        ProductCreateDto productCreateDto = new()
        {
            CategoryId = 2,
            Name = "Created Product"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(productCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        ProductDto? product = JsonSerializer.Deserialize<ProductDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.NotNull(httpResponse.Headers.Location);
        Assert.NotNull(product);
        Assert.Equal(productCreateDto.CategoryId, product.CategoryId);
        Assert.Equal(productCreateDto.Name, product.Name);
    }
    [Fact]
    public async Task UpdateProduct_ShouldWork()
    {
        int productId = 4;
        HttpRequestMessage requestMessage = new(HttpMethod.Put, $"api/Products/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        ProductUpdateDto productUpdateDto = new()
        {
            CategoryId = 2,
            Name = "Updated Product"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(productUpdateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        ProductDto? product = JsonSerializer.Deserialize<ProductDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(product);
        Assert.Equal(productUpdateDto.Name, product.Name);
    }
    [Fact]
    public async Task AddProductInstance_ShouldWork()
    {
        int productId = 6;
        HttpRequestMessage requestMessage = new(HttpMethod.Post, $"api/Products/{productId}/productInstances");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        ProductInstanceCreateDto productInstanceCreateDto = new()
        {
            IsNewCollection = false,
            StockQuantity = 100,
            Price = 75m,
            Size = "M",
            SKU = "1234",
            Status = "Abc",
            Material = "Mat",
            AbsoluteDiscount = 0,
            PercentageDiscount = 0,
            Color = "red"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(productInstanceCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        ProductInstanceDto? productInstance = JsonSerializer.Deserialize<ProductInstanceDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Product? product = await context!.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == productId);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(productInstance);
        Assert.Equal(productInstanceCreateDto.Price, productInstance.Price);
        Assert.NotNull(product);
        Assert.Equal(3, product.ProductInstances.Count);
    }
    [Fact]
    public async Task UpdateProductInstance_ShouldWork()
    {
        int productId = 6;
        int productInstanceId = 11;
        HttpRequestMessage requestMessage = new(HttpMethod.Put, $"api/Products/{productId}/{productInstanceId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        ProductInstanceCreateDto productInstanceUpdateDto = new()
        {
            StockQuantity = 90,
            Price = 100,
            AbsoluteDiscount = 0,
            Material = "Updated material"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(productInstanceUpdateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        ProductInstanceDto? productInstance = JsonSerializer.Deserialize<ProductInstanceDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(productInstance);
        Assert.Equal(productInstanceUpdateDto.Material, productInstance.Material);
    }
    [Fact]
    public async Task DeleteProductInstance_ShouldWork()
    {
        int productId = 7;
        int productInstanceId = 14;
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/Products/{productId}/{productInstanceId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Product? product = await context!.Products.FirstOrDefaultAsync(p => p.Id == productId);
        ProductInstance? productInstance = product?.ProductInstances.Find(pi => pi.Id == productInstanceId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.NotNull(product);
        Assert.Null(productInstance);
    }
    [Fact]
    public async Task DeleteProduct_ShouldWork()
    {
        int productId = 8;
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/Products/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Product? product = await context!.Products.FirstOrDefaultAsync(p => p.Id == productId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.Null(product);
    }
    [Fact]
    public async Task GetAllFeedback_ShouldReturnAllFeedback()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Products/feedback");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductFeedbackDto>? feedbacks = JsonSerializer.Deserialize<IEnumerable<ProductFeedbackDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(feedbacks);
        Assert.NotEmpty(feedbacks);
        Assert.True(feedbacks.Count() >= 3);
    }
    [Fact]
    public async Task GetProductFeedback_ShouldReturnFeedback()
    {
        int productId = 3;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Products/feedback/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductFeedbackDto>? feedbacks = JsonSerializer.Deserialize<IEnumerable<ProductFeedbackDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(feedbacks);
        Assert.Single(feedbacks);
    }
    [Fact]
    public async Task AddFeedback_ShouldWork()
    {
        int productId = 4;
        HttpRequestMessage requestMessage = new(HttpMethod.Post, $"api/Products/feedback/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "Автор",
            Rating = 1,
            Review = "Відгук"
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(productFeedbackCreateDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Product? product = await context!.Products
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.NotNull(product);
        Assert.Single(product.Feedbacks);
    }
    [Fact]
    public async Task DeleteFeedback_ShouldWork()
    {
        int productId = 5;
        int feedbackId = 5;
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/Products/feedback/{productId}/{feedbackId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        Product? product = await context!.Products.FirstOrDefaultAsync(p => p.Id == productId);
        ProductFeedback? feedback = product?.Feedbacks.Find(f => f.Id == feedbackId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.NotNull(product);
        Assert.Null(feedback);
    }
}
