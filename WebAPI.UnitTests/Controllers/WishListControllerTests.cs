using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.BLL.Models.WishLists;
using HM.DAL.Constants;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class WishListControllerTests
{
    private readonly IWishListService _wishListService;
    private readonly IStatisticsService _statisticsService;
    private readonly WishListController _wishListController;
    public WishListControllerTests()
    {
        _wishListService = Substitute.For<IWishListService>();
        _statisticsService = Substitute.For<IStatisticsService>();
        _wishListController = new WishListController(_wishListService, _statisticsService);
    }
    [Fact]
    public async Task GetWishList_ShouldReturnOkResult_WhenWishListExists()
    {
        _wishListService.GetWishListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WishListDto());

        ActionResult<WishListDto> response = await _wishListController
            .GetWishList("1", CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetWishList_ShouldReturnNotFound_WhenWishListDoesNotExist()
    {
        _wishListService.GetWishListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WishListDto?)null);

        ActionResult<WishListDto> response = await _wishListController
            .GetWishList("1", CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task GetMyWishList_ShouldReturnOkResult_WhenWishListExists()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.GetWishListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WishListDto());

        ActionResult<WishListDto> response = await _wishListController
            .GetMyWishList(CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetMyWishList_ShouldReturnUnauthorizedResult_WhenUserIsNotUnauthenticated()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!,
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.GetWishListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WishListDto());

        ActionResult<WishListDto> response = await _wishListController
            .GetMyWishList(CancellationToken.None);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task GetMyWishList_ShouldReturnNotFound_WhenWishListDoesNotExist()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.GetWishListAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WishListDto?)null);

        ActionResult<WishListDto> response = await _wishListController
            .GetMyWishList(CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task AddProductToWishList_ShouldReturnOkResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.AddProductToWishListAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(true, new WishListDto()));

        ActionResult<WishListDto> response = await _wishListController
            .AddProductToWishList(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task AddProductToWishList_ShouldReturnUnauthorizedResult_WhenUserIsNotUnauthenticated()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.AddProductToWishListAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(true, new WishListDto()));

        ActionResult<WishListDto> response = await _wishListController
            .AddProductToWishList(1, CancellationToken.None);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task AddProductToWishList_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.AddProductToWishListAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(false, "Failed!"));

        ActionResult<WishListDto> response = await _wishListController
            .AddProductToWishList(1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task RemoveProductFromWishList_ShouldReturnOkResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.RemoveProductFromWishListAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(true, new WishListDto()));

        ActionResult<WishListDto> response = await _wishListController
            .RemoveProductFromWishList(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task RemoveProductFromWishList_ShouldReturnUnauthorizedResult_WhenUserIsNotUnauthenticated()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.RemoveProductFromWishListAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(true, new WishListDto()));

        ActionResult<WishListDto> response = await _wishListController
            .RemoveProductFromWishList(1, CancellationToken.None);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task RemoveProductFromWishList_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _wishListController);
        _wishListService.RemoveProductFromWishListAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<WishListDto>(false, "Failed!"));

        ActionResult<WishListDto> response = await _wishListController
            .RemoveProductFromWishList(1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
