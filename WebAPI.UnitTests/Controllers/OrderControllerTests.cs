using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class OrderControllerTests
{
    private readonly IOrderService _orderService;
    private readonly IStatisticsService _statisticsService;
    private readonly IEmailService _emailService;
    private readonly OrdersController _ordersController;
    public OrderControllerTests()
    {
        _orderService = Substitute.For<IOrderService>();
        _statisticsService = Substitute.For<IStatisticsService>();
        _emailService = Substitute.For<IEmailService>();
        _ordersController = new OrdersController(_orderService, _statisticsService, _emailService);
    }
    [Fact]
    public async Task GetAllOrders_ShouldReturnOkResult()
    {
        _orderService.GetOrdersAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<OrderDto>> response = await _ordersController.GetAllOrders();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetUserOrders_ShouldReturnOkResultWithCurrentUserOrders()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.GetOrdersAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<OrderDto>> response = await _ordersController
            .GetUserOrders(CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetUserOrders_ShouldReturnBadRequest_WhenCannotGetUserId()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.GetOrdersAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<OrderDto>> response = await _ordersController
            .GetUserOrders(CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task GetOrderById_ShouldReturnOkResult_WhenOrderExist()
    {
        _orderService.GetOrderByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new OrderDto());

        ActionResult<OrderDto> response = await _ordersController.GetOrderById(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        _orderService.GetOrderByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((OrderDto?)null);

        ActionResult<OrderDto> response = await _ordersController.GetOrderById(1, CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task CreateOrder_ShouldReturnCreatedResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.CreateOrderAsync(Arg.Any<OrderCreateDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));

        ActionResult<OrderDto> response = await _ordersController
            .CreateOrder(new OrderCreateDto(), CancellationToken.None);
        var result = response.Result as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CreateOrder_ShouldReturnUnauthorizedWhenCalledByUnauthenticatedUser()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.CreateOrderAsync(Arg.Any<OrderCreateDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));

        ActionResult<OrderDto> response = await _ordersController
            .CreateOrder(new OrderCreateDto(), CancellationToken.None);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task CreateOrder_ShouldSendEmail_WhenSendEmailIsTrue()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.CreateOrderAsync(Arg.Any<OrderCreateDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));
        _emailService.SendOrderCreatedEmailAsync(Arg.Any<OrderDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult<OrderDto> response = await _ordersController
            .CreateOrder(new OrderCreateDto(), CancellationToken.None, true);

        Assert.NotNull(response);
        await _emailService.Received()
            .SendOrderCreatedEmailAsync(Arg.Any<OrderDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task CreateOrder_ShouldModifyProductStatistics()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.CreateOrderAsync(Arg.Any<OrderCreateDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));

        ActionResult<OrderDto> response = await _ordersController
            .CreateOrder(new OrderCreateDto(), CancellationToken.None);

        Assert.NotNull(response);
        await _statisticsService.Received().AddToProductNumberPurchasesAsync(Arg.Any<OrderDto>());
    }
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _ordersController);
        _orderService.CreateOrderAsync(Arg.Any<OrderCreateDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(false, "Failed!"));

        ActionResult<OrderDto> response = await _ordersController
            .CreateOrder(new OrderCreateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateOrderStatus_ShouldReturnOkResult_WhenSucceeded()
    {
        _orderService.UpdateOrderAsync(Arg.Any<int>(), Arg.Any<OrderUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));

        ActionResult<OrderDto> response = await _ordersController
            .UpdateOrderStatus(1, new OrderUpdateDto(), CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task UpdateOrderStatus_ShouldSendEmail_WhenSendEmailIsTrue()
    {
        _orderService.UpdateOrderAsync(Arg.Any<int>(), Arg.Any<OrderUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(true, new OrderDto()));

        ActionResult<OrderDto> response = await _ordersController
            .UpdateOrderStatus(1, new OrderUpdateDto(), CancellationToken.None, true);

        Assert.NotNull(response);
        await _emailService.Received()
            .SendOrderStatusUpdatedEmailAsync(Arg.Any<OrderDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task UpdateOrderStatus_ShouldReturnBadRequest_WhenFailed()
    {
        _orderService.UpdateOrderAsync(Arg.Any<int>(), Arg.Any<OrderUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<OrderDto>(false, "Failed!"));
        _emailService.SendOrderStatusUpdatedEmailAsync(Arg.Any<OrderDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult<OrderDto> response = await _ordersController
            .UpdateOrderStatus(1, new OrderUpdateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
