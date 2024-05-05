using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class CheckoutControllerTests
{
    private readonly ICheckoutService _checkoutService;
    private readonly CheckoutController _checkoutController;
    public CheckoutControllerTests()
    {
        _checkoutService = Substitute.For<ICheckoutService>();
        _checkoutController = new CheckoutController(_checkoutService);
    }
    [Fact]
    public async Task CheckoutOrder_ShouldReturnOkResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockHost(_checkoutController);
        ControllerHelper.MockUserIdentity(userDto, _checkoutController);
        _checkoutService.PayForOrderAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<string>(true, "", "token"));

        ActionResult<LinkDto> response = await _checkoutController.CheckoutOrder(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CheckoutOrder_ShouldReturnUnauthorized_WhenCalledByUnauthenticatedUser()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockHost(_checkoutController);
        ControllerHelper.MockUserIdentity(userDto, _checkoutController);
        _checkoutService.PayForOrderAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<string>(true, "", "token"));

        ActionResult<LinkDto> response = await _checkoutController.CheckoutOrder(1, CancellationToken.None);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task CheckoutOrder_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockHost(_checkoutController);
        ControllerHelper.MockUserIdentity(userDto, _checkoutController);
        _checkoutService.PayForOrderAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<string>(false, "Failed!", null!));

        ActionResult<LinkDto> response = await _checkoutController.CheckoutOrder(1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task CheckoutSucceeded_ShouldReturnRedirectResult()
    {
        _checkoutService.CheckoutSuccessAsync(Arg.Any<string>()).Returns(new OperationResult(true));

        ActionResult response = await _checkoutController.CheckoutSucceeded("session1");

        Assert.IsType<RedirectResult>(response);
    }
    [Fact]
    public async Task CheckoutSucceeded_ShouldReturnBadResult_WhenCheckoutHasNotBeenProcessedCorrectly()
    {
        _checkoutService.CheckoutSuccessAsync(Arg.Any<string>()).Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _checkoutController.CheckoutSucceeded("session1");
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task CheckoutFailed_ShouldReturnRedirectResult()
    {
        ActionResult response = _checkoutController.CheckoutFailed();

        Assert.IsType<RedirectResult>(response);
    }
}
