using HM.BLL.Models.Common;
using HM.BLL.Models.WishLists;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Services;

public class WishListServiceTests
{
    private readonly HmDbContext _context;
    private readonly ILogger<WishListService> _logger;
    private readonly WishListService _wishListService;
    public WishListServiceTests()
    {
        _context = ServiceHelper.GetTestDbContext();
        _logger = Substitute.For<ILogger<WishListService>>();
        _wishListService = new WishListService(_context, _logger);
    }
    [Fact]
    public async Task GetWishListAsync_ShouldReturnWishListOfTheUser()
    {
        await SeedDbContextAsync();

        WishListDto? wishList = await _wishListService.GetWishListAsync("1", CancellationToken.None);

        Assert.NotNull(wishList);
        Assert.Equal(1, wishList.Id);
        Assert.Equal(2, wishList.Products.Count);
    }
    [Fact]
    public async Task GetWishListAsync_ShouldReturnNull_WhenUserDoesNotHaveWishList()
    {
        await SeedDbContextAsync();

        WishListDto? wishList = await _wishListService.GetWishListAsync("999", CancellationToken.None);

        Assert.Null(wishList);
    }
    [Fact]
    public async Task AddProductToWishListAsync_ShouldAddProductToTheWishList()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .AddProductToWishListAsync("2", 2, CancellationToken.None);
        WishList? wishList = await _context.WishLists.FirstOrDefaultAsync(w => w.UserId == "2");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Payload.Products.Count);
        Assert.NotNull(wishList);
        Assert.Equal(2, wishList.Products.Count);
    }
    [Fact]
    public async Task AddProductToWishListAsync_ShouldCreateWishListIfItDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .AddProductToWishListAsync("100", 1, CancellationToken.None);
        WishList? wishList = await _context.WishLists.FirstOrDefaultAsync(w => w.UserId == "100");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Single(result.Payload.Products);
        Assert.NotNull(wishList);
        Assert.Single(wishList.Products);
    }
    [Fact]
    public async Task AddProductToWishListAsync_ShouldReturnFalseResult_WhenTheWishListAlreadyContainsTheProduct()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .AddProductToWishListAsync("1", 1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.False(result.Succeeded);
        Assert.True(result.Payload.Products.Exists(p => p.Id == 1));
    }
    [Fact]
    public async Task AddProductToWishListAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .AddProductToWishListAsync("1", 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task AddProductToWishListAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new WishListService(dbContextMock, _logger);

        OperationResult<WishListDto> result = await service
            .AddProductToWishListAsync("2", 2, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RemoveProductFromWishListAsync_ShouldRemoveProductFromTheWishList()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .RemoveProductFromWishListAsync("1", 1, CancellationToken.None);
        WishList? wishList = await _context.WishLists.FirstOrDefaultAsync(w => w.UserId == "2");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Single(result.Payload.Products);
        Assert.NotNull(wishList);
        Assert.Single(wishList.Products);
    }
    [Fact]
    public async Task RemoveProductFromWishListAsync_ShouldReturnFalseResult_WhenWishListDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .RemoveProductFromWishListAsync("999", 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task RemoveProductFromWishListAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .RemoveProductFromWishListAsync("1", 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.Equal(2, result.Payload.Products.Count);
    }
    [Fact]
    public async Task RemoveProductFromWishListAsync_ShouldReturnFalseResult_WhenWishListDoesNotContainTheProduct()
    {
        await SeedDbContextAsync();

        OperationResult<WishListDto> result = await _wishListService
            .RemoveProductFromWishListAsync("2", 2, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.Single(result.Payload.Products);
    }
    [Fact]
    public async Task RemoveProductFromWishListAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new WishListService(dbContextMock, _logger);

        OperationResult<WishListDto> result = await service
            .RemoveProductFromWishListAsync("1", 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        List<Product> products = Products;
        await context.CategoryGroups.AddAsync(CategoryGroup);
        await context.Categories.AddAsync(Category);
        await context.Products.AddRangeAsync(products);
        List<WishList> wishLists = WishLists;
        await context.WishLists.AddRangeAsync(wishLists);
        await context.SaveChangesAsync();
        wishLists[0].Products.Add(products[0]);
        wishLists[0].Products.Add(products[1]);
        wishLists[1].Products.Add(products[0]);
        await context.SaveChangesAsync();
    }
    private static CategoryGroup CategoryGroup => new()
    {
        Id = 1,
        Name = "Category group 1"
    };
    private static Category Category => new()
    {
        Id = 1,
        Name = "Category 1",
        CategoryGroupId = 1
    };

    private static List<Product> Products =>
    [
        new()
        {
            Id = 1,
            CategoryId = 1,
            Name = "Product 1",
            Description = "Description 1",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances = [],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics = []
        },
        new()
        {
            Id = 2,
            CategoryId = 1,
            Name = "Product 2",
            Description = "Description 2",
            Rating = 0.6m,
            TimesRated = 10,
            ProductInstances = [],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics = []
        }
    ];

    private static List<WishList> WishLists =>
    [
        new()
        {
            Id = 1,
            UserId = "1",
            Products = []
        },
        new()
        {
            Id = 2,
            UserId = "2",
            Products = []
        },
        new()
        {
            Id = 3,
            UserId = "3",
            Products = []
        },
    ];
}
