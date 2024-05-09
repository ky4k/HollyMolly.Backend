using HM.BLL.Models.WishLists;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using WebAPI.IntegrationTests.TestHelpers;
using WebAPI.IntegrationTests.WebApplicationFactory;

namespace WebAPI.IntegrationTests.Endpoints;

public class WishListIntegrationTests : IClassFixture<SharedWebAppFactory>
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly SharedWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly AuthorizationHelper _authorizationHelper;
    public WishListIntegrationTests(SharedWebAppFactory factory)
    {
        _factory = factory;
        _factory.Initialize();
        _factory.SeedContextAsync(SeedDefaultEntities.SeedAsync).WaitAsync(CancellationToken.None);
        _httpClient = factory.CreateClient();
        _authorizationHelper = new AuthorizationHelper(_httpClient);
    }
    [Fact]
    public async Task GetWishList_ShouldReturnWishListOfTheSpecifiedUser()
    {
        string userId = "2";
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/WishList/{userId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("admin1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        WishListDto? wishListDto = JsonSerializer.Deserialize<WishListDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(wishListDto);
    }
    [Fact]
    public async Task GetMyWishList_ShouldReturnWishListOfTheCurrentUser()
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/WishList/myWishList");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        WishListDto? wishListDto = JsonSerializer.Deserialize<WishListDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(wishListDto);
    }
    [Fact]
    public async Task AddProductToWishList_ShouldWork()
    {
        int productId = 3;
        HttpRequestMessage requestMessage = new(HttpMethod.Post, $"api/WishList/myWishList/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user1@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        WishListDto? wishListDto = JsonSerializer.Deserialize<WishListDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);

        Assert.NotNull(wishListDto);
        Assert.Single(wishListDto.Products);
        Assert.Equal(3, wishListDto.Products[0].Id);
    }
    [Fact]
    public async Task RemoveProductFromWishList_ShouldWork()
    {
        int productId = 1;
        using (var scope1 = _factory.CreateScope())
        {
            var context = scope1.ServiceProvider.GetService<HmDbContext>();
            WishList userWishList = await context!.WishLists.Include(w => w.Products)
                .FirstAsync(w => w.UserId == "2");
            userWishList.Products.Add(await context.Products.FirstAsync(p => p.Id == productId));
            await context.SaveChangesAsync();
        }
        HttpRequestMessage requestMessage = new(HttpMethod.Delete, $"api/WishList/myWishList/{productId}");
        requestMessage.Headers.Authorization = await _authorizationHelper
            .GetAuthorizationHeaderAsync("user2@example.com", "password");

        HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
        httpResponse.EnsureSuccessStatusCode();
        WishListDto? wishListDto = JsonSerializer.Deserialize<WishListDto>(
            httpResponse.Content.ReadAsStream(), jsonSerializerOptions);
        WishList? wishList = null;
        using (var scope2 = _factory.CreateScope())
        {
            var context = scope2.ServiceProvider.GetService<HmDbContext>();
            wishList = await context!.WishLists.Include(w => w.Products)
                .FirstOrDefaultAsync(w => w.UserId == "2");
        }

        Assert.NotNull(wishListDto);
        Assert.Empty(wishListDto.Products);
        Assert.NotNull(wishList);
        Assert.Empty(wishList.Products);
    }
}
