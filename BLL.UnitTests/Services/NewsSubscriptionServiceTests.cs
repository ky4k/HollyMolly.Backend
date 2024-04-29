using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Services;

public class NewsSubscriptionServiceTests
{
    private readonly HmDbContext _context;
    private readonly ILogger<NewsSubscriptionService> _logger;
    private readonly NewsSubscriptionService _newsSubscriptionService;
    public NewsSubscriptionServiceTests()
    {
        _context = ServiceHelper.GetTestDbContext();
        _logger = Substitute.For<ILogger<NewsSubscriptionService>>();
        _newsSubscriptionService = new NewsSubscriptionService(_context, _logger);
    }
    [Fact]
    public async Task GetAllSubscriptionsAsync_ShouldReturnAllSubscriptions()
    {
        await SeedDbContextAsync();

        IEnumerable<NewsSubscriptionDto> newsSubscriptions = await _newsSubscriptionService
            .GetAllSubscriptionsAsync(CancellationToken.None);

        Assert.Equal(3, newsSubscriptions.Count());
    }
    [Fact]
    public async Task GetAllSubscriptionsAsync_ShouldReturnEmptyList_WhenThereAreNoSubscriptions()
    {
        IEnumerable<NewsSubscriptionDto> newsSubscriptions = await _newsSubscriptionService
            .GetAllSubscriptionsAsync(CancellationToken.None);

        Assert.Empty(newsSubscriptions);
    }
    [Fact]
    public async Task AddSubscriptionAsync_ShouldAddNewSubscription()
    {
        await SeedDbContextAsync();
        NewsSubscriptionCreateDto newsSubscriptionDto = new()
        {
            Email = "new1@example.com"
        };

        OperationResult result = await _newsSubscriptionService
            .AddSubscriptionAsync(newsSubscriptionDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task AddSubscriptionAsync_ShouldNotAddSubscription_WhenItAlreadyExists()
    {
        await SeedDbContextAsync();
        NewsSubscriptionCreateDto newsSubscriptionDto = new()
        {
            Email = "test1@example.com"
        };

        OperationResult result = await _newsSubscriptionService
            .AddSubscriptionAsync(newsSubscriptionDto, CancellationToken.None);
        int numberSubscriptions = await _context.NewsSubscriptions.CountAsync();

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(3, numberSubscriptions);
    }
    [Fact]
    public async Task AddSubscriptionAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new NewsSubscriptionService(dbContextMock, _logger);
        NewsSubscriptionCreateDto newsSubscriptionDto = new()
        {
            Email = "new1@example.com"
        };

        OperationResult result = await service
            .AddSubscriptionAsync(newsSubscriptionDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RemoveSubscriptionAsync_ShouldRemoveExistingSubscription()
    {
        await SeedDbContextAsync();

        OperationResult result = await _newsSubscriptionService
            .RemoveSubscriptionAsync("1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RemoveSubscriptionAsync_ShouldReturnTrueResult_WhenSubscriptionDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _newsSubscriptionService
            .RemoveSubscriptionAsync("999", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RemoveSubscriptionAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new NewsSubscriptionService(dbContextMock, _logger);

        OperationResult result = await service
            .RemoveSubscriptionAsync("1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.NewsSubscriptions.AddRangeAsync(NewsSubscriptions);
        await context.SaveChangesAsync();
    }

    private static List<NewsSubscription> NewsSubscriptions =>
    [
        new()
        {
            Id = 1,
            Email = "test1@example.com",
            RemoveToken = "1"
        },
        new()
        {
            Id = 2,
            Email = "test2@example.com",
            RemoveToken = "2"
        },
        new()
        {
            Id = 3,
            Email = "test3@example.com",
            RemoveToken = "3"
        }
    ];
}
