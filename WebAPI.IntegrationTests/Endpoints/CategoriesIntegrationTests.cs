using HM.BLL.Models.Categories;
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

public class CategoriesIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public CategoriesIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetAllCategories_ShouldReturnAllCategories()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/Categories");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<CategoryGroupDto>? categories = JsonSerializer.Deserialize<IEnumerable<CategoryGroupDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
        Assert.Equal(3, categories.Count());
        Assert.Equal(2, categories.FirstOrDefault()?.Categories.Count);
    }
    [Fact]
    public async Task GetCategoryGroup_ShouldReturnCategory()
    {
        int categoryGroupId = 1;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Categories/{categoryGroupId}");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        CategoryGroupDto? categoryGroup = JsonSerializer.Deserialize<CategoryGroupDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(categoryGroup);
        Assert.Equal(2, categoryGroup.Categories.Count);
    }
    [Fact]
    public async Task GetCategoryGroupProducts_ShouldReturnProducts()
    {
        int categoryGroupId = 1;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Categories/{categoryGroupId}/products");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        IEnumerable<ProductDto>? products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Equal(4, products.Count());
        Assert.Equal("Product 1", products.FirstOrDefault()?.Name);
    }

    [Fact]
    public async Task CreateCategoryGroup_ShouldCreateCategoryGroup()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Categories");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "New category group",
            Position = 10
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(categoryGroupDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        CategoryGroupDto? categoryGroup = JsonSerializer.Deserialize<CategoryGroupDto>(
            await httpResponse.Content.ReadAsStringAsync(), jsonSerializerOptions);

        httpResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.NotNull(httpResponse.Headers.Location);
        Assert.NotNull(categoryGroup);
        Assert.Equal(categoryGroupDto.Name, categoryGroup.Name);
    }
    [Fact]
    public async Task UpdateCategoryGroup_ShouldWork()
    {
        int categoryGroupId = 2;
        HttpRequestMessage requestMessage = new(HttpMethod.Put, $"api/Categories/{categoryGroupId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "Updated group",
            Position = 10
        };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(categoryGroupDto),
            Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        CategoryGroup? categoryGroup = await context!.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.NotNull(categoryGroup);
        Assert.Equal(categoryGroupDto.Name, categoryGroup.Name);
        Assert.Equal(categoryGroupDto.Position, categoryGroup.Position);
    }
    [Fact]
    public async Task DeleteCategoryGroup_ShouldWork()
    {
        int categoryGroupId = 3;
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/Categories/{categoryGroupId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetService<HmDbContext>();
        CategoryGroup? categoryGroup = await context!.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId);

        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
        Assert.Null(categoryGroup);
    }
}
