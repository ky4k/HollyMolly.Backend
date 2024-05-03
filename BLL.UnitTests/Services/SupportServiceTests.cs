using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using HM.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Services;

public class SupportServiceTests
{
    private readonly HmDbContext _context;
    private readonly ILogger<SupportService> _logger;
    private readonly SupportService _supportService;
    public SupportServiceTests()
    {
        _context = ServiceHelper.GetTestDbContext();
        _logger = Substitute.For<ILogger<SupportService>>();
        _supportService = new SupportService(_context, _logger);
    }
    [Fact]
    public async Task SaveSupportRequestAsync_ShouldAddSupportToTheDatabase()
    {
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.ProductQuestions
        };

        OperationResult result = await _supportService.SaveSupportRequestAsync(
            supportDto, CancellationToken.None);
        Support? support = await _context.Supports.FirstOrDefaultAsync(s => s.Email == supportDto.Email);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(support);
        Assert.Equal(supportDto.Name, support.Name);
        Assert.Equal(supportDto.Description, support.Description);
    }
    [Fact]
    public async Task SaveSupportRequestAsync_ShouldAddOrderId_IfOrderExist()
    {
        await _context.Orders.AddAsync(Order);
        await _context.SaveChangesAsync();
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.PaymentQuestions,
            OrderId = 1
        };

        OperationResult result = await _supportService.SaveSupportRequestAsync(
            supportDto, CancellationToken.None);
        Support? support = await _context.Supports.FirstOrDefaultAsync(s => s.Email == supportDto.Email);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(support);
        Assert.Equal(1, support.OrderId);
    }
    [Fact]
    public async Task SaveSupportRequestAsync_ShouldNotAddOrderId_IfOrderDoesNotExist()
    {
        await _context.Orders.AddAsync(Order);
        await _context.SaveChangesAsync();
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.PaymentQuestions,
            OrderId = 2
        };

        OperationResult result = await _supportService.SaveSupportRequestAsync(
            supportDto, CancellationToken.None);
        Support? support = await _context.Supports.FirstOrDefaultAsync(s => s.Email == supportDto.Email);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(support);
        Assert.Null(support.OrderId);
    }
    [Fact]
    public async Task SaveSupportRequestAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new SupportService(dbContextMock, _logger);
        SupportCreateDto supportDto = new()
        {
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.Other
        };

        OperationResult result = await service.SaveSupportRequestAsync(
            supportDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SupportEntity_ShouldContain_CorrectOrder()
    {
        await _context.Supports.AddAsync(Support);
        await _context.SaveChangesAsync();

        var support = await _context.Supports.FirstOrDefaultAsync(s => s.Id == 1);

        Assert.NotNull(support);
        Assert.Equal(1, support.OrderId);
    }

    private static Order Order => new()
    {
        Id = 1,
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
        Status = "Default",
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

    private static Support Support => new()
    {
        Id = 1,
        Email = "test@example.com",
        Name = "Test",
        Description = "My problem",
        Order = Order
    };
}
