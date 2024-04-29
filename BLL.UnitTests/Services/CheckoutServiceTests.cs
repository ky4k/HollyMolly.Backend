using HM.BLL.Models.Common;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stripe.Checkout;

namespace HM.BLL.UnitTests.Services;

public class CheckoutServiceTests
{
    private readonly HmDbContext _context;
    private readonly SessionService _sessionService;
    private readonly ILogger<CheckoutService> _logger;
    private readonly CheckoutService _checkoutService;
    public CheckoutServiceTests()
    {
        _context = ServiceHelper.GetTestDbContext();
        _sessionService = Substitute.For<SessionService>();
        _sessionService.CreateAsync(Arg.Any<SessionCreateOptions>()).Returns(Session);
        _logger = Substitute.For<ILogger<CheckoutService>>();
        _checkoutService = new CheckoutService(_context, _sessionService, _logger);
    }

    [Fact]
    public async Task PayForOrderAsync_ShouldReturnRedirectUrl_WhenAllInformationIsCorrect()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(1, "1", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal("TestUrl", result.Payload);
    }
    [Fact]
    public async Task PayForOrderAsync_ShouldReturnFalseResult_WhenOrderDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(5, "1", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task PayForOrderAsync_ShouldReturnFalseResult_WhenOrderDoesNotBelongToUser()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(1, "999", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task PayForOrderAsync_ShouldReturnFalseResult_WhenOrderHasBeenAlreadyPaidFor()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(3, "1", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task PayForOrderAsync_ShouldReturnFalseResult_WhenOrderPriceLessThanMinimum()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(4, "1", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task PayForOrderAsync_ShouldReturnFalseResult_WhenSessionWasNotCreated()
    {
        await SeedDbContextAsync();
        _sessionService.CreateAsync(Arg.Any<SessionCreateOptions>())
            .ThrowsAsync<InvalidOperationException>();

        OperationResult<string> result = await _checkoutService
            .PayForOrderAsync(1, "1", "baseUrl", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task CheckoutSuccessAsync_ShouldUpdateOrderStatus()
    {
        await SeedDbContextAsync();
        _sessionService.GetAsync(Arg.Any<string>()).Returns(Session);

        OperationResult result = await _checkoutService.CheckoutSuccessAsync("1");
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(order);
        Assert.Equal("Payment Received", order.Status);
        Assert.True(order.PaymentReceived);
    }
    [Fact]
    public async Task CheckoutSuccessAsync_ShouldReturnFalseResult_WhenPaymentWasNotReceived()
    {
        await SeedDbContextAsync();
        Session session = Session;
        session.PaymentStatus = "unpaid";
        _sessionService.GetAsync(Arg.Any<string>()).Returns(session);

        OperationResult result = await _checkoutService.CheckoutSuccessAsync("1");
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == 1);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(order);
        Assert.NotEqual("Payment Received", order.Status);
        Assert.False(order.PaymentReceived);
    }
    [Fact]
    public async Task CheckoutSuccessAsync_ShouldReturnFalseResult_WhenOrderDoesNotExist()
    {
        await SeedDbContextAsync();
        Session session = Session;
        session.Metadata["orderId"] = "999";
        _sessionService.GetAsync(Arg.Any<string>()).Returns(session);

        OperationResult result = await _checkoutService.CheckoutSuccessAsync("1");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task CheckoutSuccessAsync_ShouldReturnFalseResult_WhenChangesWereNotSavedCorrectly()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        _sessionService.GetAsync(Arg.Any<string>()).Returns(Session);
        var service = new CheckoutService(dbContextMock, _sessionService, _logger);

        OperationResult result = await service.CheckoutSuccessAsync("1");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }
    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.CategoryGroups.AddAsync(CategoryGroup);
        await context.Categories.AddAsync(Category);
        await context.Products.AddRangeAsync(Products);
        await context.Orders.AddRangeAsync(Orders);
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
        CategoryGroupId = 1,
        Name = "Category 1"
    };

    private static List<Product> Products =>
    [
        new()
        {
            Id = 1,
            Name = "Product 1",
            CategoryId = 1,
            ProductInstances =
            [
                new()
                {
                    Id = 1,
                    Price = 100,
                    StockQuantity = 100,
                    Images =
                    [
                        new()
                        {
                            Id = 1,
                            FilePath = "path1",
                            Link = "link1"
                        },
                        new()
                        {
                            Id = 2,
                            FilePath = "path1a",
                            Link = "link1a"
                        }
                    ]
                }
            ]
        },
        new()
        {
            Id = 2,
            Name = "Product 2",
            CategoryId = 1,
            ProductInstances =
            [
                new()
                {
                    Id = 2,
                    Price = 50,
                    StockQuantity = 100
                }
            ]
        },
        new()
        {
            Id = 3,
            Name = "Product 3",
            CategoryId = 1,
            ProductInstances =
            [
                new()
                {
                    Id = 3,
                    Price = 1,
                    StockQuantity = 100
                }
            ]
        }
    ];

    private static List<Order> Orders =>
    [
        new()
        {
            Id = 1,
            UserId = "1",
            PaymentReceived = false,
            Status = "Created",
            OrderDate = new DateTimeOffset(2024, 04, 26, 12, 0, 0, TimeSpan.Zero),
            OrderRecords =
            [
                new()
                {
                    Id = 1,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    Discount = 10,
                },
                new()
                {
                    Id = 2,
                    ProductName = "Product 2",
                    ProductInstanceId = 2,
                    Quantity = 2,
                    Price = 50,
                    Discount = 10
                },
            ]
        },
        new()
        {
            Id = 2,
            UserId = "1",
            PaymentReceived = false,
            Status = "Created",
            OrderDate = new DateTimeOffset(2024, 04, 26, 12, 0, 0, TimeSpan.Zero),
            OrderRecords =
            [
                new()
                {
                    Id = 3,
                    ProductName = "Product 3",
                    ProductInstanceId = 3,
                    Quantity = 1,
                    Price = 1,
                    Discount = 0
                }
            ]
        },
        new()
        {
            Id = 3,
            UserId = "1",
            PaymentReceived = true,
            Status = "Created",
            OrderDate = new DateTimeOffset(2024, 04, 26, 12, 0, 0, TimeSpan.Zero),
            OrderRecords =
            [
                new()
                {
                    Id = 4,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    Discount = 0
                }
            ]
        },
        new()
        {
            Id = 4,
            UserId = "1",
            PaymentReceived = false,
            Status = "Created",
            OrderDate = new DateTimeOffset(2024, 04, 26, 12, 0, 0, TimeSpan.Zero),
            OrderRecords =
            [
                new()
                {
                    Id = 5,
                    ProductName = "Product 3",
                    ProductInstanceId = 4,
                    Quantity = 0,
                    Price = 1,
                    Discount = 0
                }
            ]
        },
    ];

    private static Session Session => new()
    {
        Id = "1",
        Url = "TestUrl",
        Metadata = new()
        {
            { "orderId", "1" }
        },
        PaymentStatus = "paid"
    };
}
