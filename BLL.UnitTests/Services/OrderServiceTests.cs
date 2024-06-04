using HM.BLL.Models.Common;
using HM.BLL.Models.Orders;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Services;

public class OrderServiceTests
{
    private readonly HmDbContext _context;
    private readonly OrderService _orderService;
    private readonly ILogger<OrderService> _logger;
    public OrderServiceTests()
    {
        _logger = Substitute.For<ILogger<OrderService>>();
        _context = ServiceHelper.GetTestDbContext();
        _orderService = new OrderService(_context, _logger);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnAllOrders()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            null, null, null, null, CancellationToken.None);

        Assert.Equal(4, orders.Count());
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldFilterOrdersByUserId()
    {
        await SeedDbContextAsync();
        string userId = "1234-5678-9012-3456";

        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            userId, null, null, null, CancellationToken.None);

        Assert.Single(orders);
    }
    [Fact]
    public async Task GetOrdersAsync_ShouldFilterOrdersByDate()
    {
        await SeedDbContextAsync();
        DateTimeOffset dateFrom = new(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset dateTo = new(2024, 3, 31, 0, 0, 0, TimeSpan.Zero);

        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            null, null, dateFrom, dateTo, CancellationToken.None);

        Assert.Single(orders);
    }
    [Fact]
    public async Task GetOrdersAsync_ShouldFilterOrdersByStatus()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            null, ["Delivered"], null, null, CancellationToken.None);

        Assert.Single(orders);
    }
    [Fact]
    public async Task GetOrdersAsync_ShouldReturnAllTheOrdersInformation()
    {
        await SeedDbContextAsync();
        string userId = "2234-5678-9012-3456";
        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            userId, null, null, null, CancellationToken.None);

        Assert.NotNull(orders);
        Assert.Equal("FirstName", orders.First().Customer.FirstName);
        Assert.Equal("Test product", orders.First().OrderRecords[0].ProductName);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnEmptyCollection_WhenThereAreNoOrders()
    {
        await SeedDbContextAsync();
        string userId = "no-user-with-such-id";
        IEnumerable<OrderDto> orders = await _orderService.GetOrdersAsync(
            userId, null, null, null, CancellationToken.None);

        Assert.Empty(orders);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnTheCorrectOrder()
    {
        await SeedDbContextAsync();
        OrderDto? order = await _orderService.GetOrderByIdAsync(2, CancellationToken.None);

        Assert.NotNull(order);
        Assert.Equal(2, order.Id);
        Assert.Equal(100, order.TotalCost);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnAllTheOrderInformation()
    {
        await SeedDbContextAsync();
        OrderDto? order = await _orderService.GetOrderByIdAsync(2, CancellationToken.None);

        Assert.NotNull(order);
        Assert.Equal("FirstName", order.Customer.FirstName);
        Assert.Equal("Test product", order.OrderRecords[0].ProductName);
        Assert.Equal(100, order.OrderRecords[0].TotalCostBeforeDiscount);
        Assert.Equal(100, order.OrderRecords[0].TotalCost);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        await SeedDbContextAsync();
        OrderDto? order = await _orderService.GetOrderByIdAsync(9999, CancellationToken.None);

        Assert.Null(order);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 1,
                    Quantity = 1
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);
        Assert.NotNull(result?.Payload);
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == result.Payload.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(order);
        Assert.True(order.OrderRecords.TrueForAll(or => or.OrderId == order.Id));
        Assert.True(order.Customer.OrderId == order.Id);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotCreateOrder_WhenNoProductAreAdded()
    {
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords = []
        };

        OperationResult<OrderDto> result = await _orderService.CreateOrderAsync(orderDto, "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotCreateOrder_WhenProductNotExist()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 9999,
                    Quantity = 1
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService.CreateOrderAsync(orderDto, "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotCreateOrder_WhenProductIsNotInTheStock()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 2,
                    Quantity = 1
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService.CreateOrderAsync(orderDto, "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotAddProductInstances_ThatIsNotInTheStock()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 1,
                    Quantity = 1
                },
                new()
                {
                    ProductInstanceId = 2,
                    Quantity = 1
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService.CreateOrderAsync(orderDto, "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload);
        Assert.Single(result.Payload.OrderRecords);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotAddMoreQuantityThanIsInStock()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 1,
                    Quantity = 200
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.OrderRecords[0]);
        Assert.Equal(100, result.Payload.OrderRecords[0].Quantity);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldApplyAbsoluteDiscountCorrectly()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 3,
                    Quantity = 100
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.OrderRecords[0]);
        Assert.Equal(90, result.Payload.OrderRecords[0].TotalCost);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldApplyPercentageDiscountCorrectly()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 4,
                    Quantity = 100
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.OrderRecords[0]);
        Assert.Equal(90, result.Payload.OrderRecords[0].TotalCost);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCombineDiscountCorrectly()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 5,
                    Quantity = 100
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.OrderRecords[0]);
        Assert.Equal(81, result.Payload.OrderRecords[0].TotalCost);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldNotCreateOrder_WhenTotalPriceIsLowerThanMinimum()
    {
        await SeedDbContextAsync();
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 3,
                    Quantity = 1
                }
            ]
        };

        OperationResult<OrderDto> result = await _orderService
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();

        var service = new OrderService(dbContextMock, _logger);
        OrderCreateDto orderDto = new()
        {
            Customer = Customer,
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 1,
                    Quantity = 1
                }
            ]
        };
        OperationResult<OrderDto> result = await service
            .CreateOrderAsync(orderDto, "1234-5678-9012-3456", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldUpdateOrder()
    {
        await SeedDbContextAsync();
        const string newStatus = "Updated";
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = newStatus
        };

        await _orderService.UpdateOrderAsync(1, orderUpdateDto, CancellationToken.None);
        Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == 1);

        Assert.NotNull(order);
        Assert.Equal(newStatus, order.Status);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldAddOrderHistoryElement()
    {
        await SeedDbContextAsync();
        const string newStatus = "Updated";
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = newStatus
        };

        await _orderService.UpdateOrderAsync(1, orderUpdateDto, CancellationToken.None);
        Order? order = await _context.Orders
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == 1);
        OrderStatusHistory? historyRecord = order?.StatusHistory.Find(s => s.OrderId == 1);

        Assert.NotNull(historyRecord?.Id);
        Assert.Equal(newStatus, historyRecord.Status);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldReturnUpdatedOrder()
    {
        await SeedDbContextAsync();
        const string newStatus = "Updated";
        const string newNotes = "New notes";
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = newStatus,
            Notes = newNotes
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            1, orderUpdateDto, CancellationToken.None);
        OrderStatusHistoryDto? status = result.Payload?.StatusHistory.MaxBy(s => s.Date);

        Assert.NotNull(result.Payload);
        Assert.NotNull(status);
        Assert.True(result.Succeeded);
        Assert.Equal(newStatus, status.Status);
        Assert.Equal(newNotes, status.Notes);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldNotSucceed_WhenOrderDoesNotExist()
    {
        await SeedDbContextAsync();
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Updated",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            9999, orderUpdateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldRestoreProductQuantity_WhenOrderIsBeingCancelled()
    {
        await SeedDbContextAsync();
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Cancelled",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            1, orderUpdateDto, CancellationToken.None);
        Product? product = await _context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == 1);
        ProductInstance? productInstance = product?.ProductInstances.Find(pi => pi.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(101, productInstance?.StockQuantity);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldRestoreProductQuantity_WhenOrderStopBeingCancelled()
    {
        await SeedDbContextAsync();
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Created",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            3, orderUpdateDto, CancellationToken.None);
        Product? product = await _context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == 1);
        ProductInstance? productInstance = product?.ProductInstances.Find(pi => pi.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(95, productInstance?.StockQuantity);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldChangeOrderRecord_WhenOrderStopBeingCancelledButNotEnoughProductsAreInStock()
    {
        await SeedDbContextAsync();
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Created",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            4, orderUpdateDto, CancellationToken.None);
        Product? product = await _context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == 1);
        ProductInstance? productInstance = product?.ProductInstances.Find(pi => pi.Id == 1);

        Assert.NotNull(result.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(100, result.Payload.OrderRecords[0].Quantity);
        Assert.Equal(0, productInstance?.StockQuantity);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldNotChangeProductQuantity_WhenOrderRemainsCancelled()
    {
        await SeedDbContextAsync();
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Cancelled",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await _orderService.UpdateOrderAsync(
            3, orderUpdateDto, CancellationToken.None);
        Product? product = await _context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == 1);
        ProductInstance? productInstance = product?.ProductInstances.Find(pi => pi.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(100, productInstance?.StockQuantity);
    }
    [Fact]
    public async Task UpdateOrderAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();

        var service = new OrderService(dbContextMock, _logger);
        OrderUpdateDto orderUpdateDto = new()
        {
            Status = "Updated",
            Notes = "New notes"
        };

        OperationResult<OrderDto> result = await service.UpdateOrderAsync(
            1, orderUpdateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }


    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.AddAsync(CategoryGroup);
        await context.AddAsync(Category);
        await context.AddAsync(Product);
        await context.AddAsync(User1);
        await context.AddAsync(User2);
        await context.AddRangeAsync(Order1, Order2, Order3, Order4);
        await context.SaveChangesAsync();
    }

    private static CustomerDto Customer => new()
    {
        FirstName = "Test",
        LastName = "Customer",
        Email = "email@example.com",
        City = "City",
        DeliveryAddress = "Address",
        PhoneNumber = "1234567890"
    };

    private static CategoryGroup CategoryGroup => new()
    {
        Id = 1,
        Name = "TestCategoryGroup"
    };
    private static Category Category => new()
    {
        Id = 1,
        CategoryGroupId = 1,
        Name = "TestCategory"
    };

    private static Product Product => new()
    {
        Id = 1,
        Name = "TestProduct",
        CategoryId = 1,
        Description = "Description",
        Feedbacks = [],
        ProductStatistics = [],
        Rating = 0,
        TimesRated = 0,
        WishLists = [],
        ProductInstances =
        [
            new()
            {
                Id = 1,
                StockQuantity = 100,
                Price = 50,
            },
            new()
            {
                Id = 2,
                StockQuantity = 0,
                Price = 70,
            },
            new()
            {
                Id = 3,
                StockQuantity = 1000,
                Price = 1,
                AbsoluteDiscount = 0.1m
            },
            new()
            {
                Id = 4,
                StockQuantity = 1000,
                Price = 1,
                PercentageDiscount = 10
            },
            new()
            {
                Id = 5,
                StockQuantity = 1000,
                Price = 1,
                AbsoluteDiscount = 0.1m,
                PercentageDiscount = 10
            }
        ]
    };

    private static User User1 => new()
    {
        Id = "1234-5678-9012-3456",
        Email = "user1@example.com",
        UserName = "user1@example.com"
    };
    private static User User2 => new()
    {
        Id = "2234-5678-9012-3456",
        Email = "user1@example.com",
        UserName = "user1@example.com"
    };

    private static Order Order1 => new()
    {
        Customer = new CustomerInfo()
        {
            Id = 1,
            Email = "email@example.com",
            FirstName = "FirstName",
            LastName = "LastName",
            City = "City",
            DeliveryAddress = "Address",
            PhoneNumber = "1234567890"
        },
        UserId = "1234-5678-9012-3456",
        OrderDate = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero),
        Status = "Created",
        OrderRecords =
        [
            new()
            {
                Id = 1,
                ProductInstanceId = 1,
                Quantity = 1,
                Price = 100,
                ProductName = "Test product"
            }
        ]
    };
    private static Order Order2 => new()
    {
        Customer = new CustomerInfo()
        {
            Id = 2,
            Email = "email@example.com",
            FirstName = "FirstName",
            LastName = "LastName",
            City = "City",
            DeliveryAddress = "Address",
            PhoneNumber = "1234567890"
        },
        UserId = "2234-5678-9012-3456",
        OrderDate = new DateTimeOffset(2024, 4, 15, 0, 0, 0, TimeSpan.Zero),
        Status = "Delivered",
        OrderRecords =
        [
            new()
            {
                Id = 2,
                ProductInstanceId = 1,
                Quantity = 1,
                Price = 100,
                ProductName = "Test product"
            }
        ]
    };
    private static Order Order3 => new()
    {
        Customer = new CustomerInfo()
        {
            Id = 3,
            Email = "email@example.com",
            FirstName = "FirstName",
            LastName = "LastName",
            City = "City",
            DeliveryAddress = "Address",
            PhoneNumber = "1234567890"
        },
        UserId = "2234-5678-9012-3456",
        OrderDate = new DateTimeOffset(2024, 4, 15, 0, 0, 0, TimeSpan.Zero),
        Status = "Cancelled",
        OrderRecords =
        [
            new()
            {
                Id = 3,
                ProductInstanceId = 1,
                Quantity = 5,
                Price = 100,
                ProductName = "Test product"
            }
        ]
    };
    private static Order Order4 => new()
    {
        Customer = new CustomerInfo()
        {
            Id = 4,
            Email = "email@example.com",
            FirstName = "FirstName",
            LastName = "LastName",
            City = "City",
            DeliveryAddress = "Address",
            PhoneNumber = "1234567890"
        },
        UserId = "2234-5678-9012-3456",
        OrderDate = new DateTimeOffset(2024, 4, 15, 0, 0, 0, TimeSpan.Zero),
        Status = "Cancelled",
        OrderRecords =
        [
            new()
            {
                Id = 4,
                ProductInstanceId = 1,
                Quantity = 125,
                Price = 100,
                ProductName = "Test product"
            }
        ]
    };
}
