using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly IUserService _userService;
    private readonly UsersController _usersController;
    public UsersControllerTests()
    {
        _userService = Substitute.For<IUserService>();
        _usersController = new UsersController(_userService);
    }
    [Fact]
    public async Task GetAllUsers_ShouldReturnOkResult()
    {
        _userService.GetUsersAsync(Arg.Any<CancellationToken>()).Returns([]);

        ActionResult<IEnumerable<UserDto>> response = await _usersController
            .GetAllUsers(CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetUserById_ShouldReturnOkResult_WhenUserExists()
    {
        _userService.GetUserByIdAsync(Arg.Any<string>()).Returns(new UserDto());

        ActionResult<UserDto> response = await _usersController.GetUserById("1");
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        _userService.GetUserByIdAsync(Arg.Any<string>()).Returns((UserDto?)null);

        ActionResult<UserDto> response = await _usersController.GetUserById("1");
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserRoles_ShouldReturnOkResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.ChangeUserRolesAsync(Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(new OperationResult<UserDto>(true, new UserDto()));

        ActionResult<UserDto> response = await _usersController
            .ChangeUserRoles("1", [DefaultRoles.Administrator, DefaultRoles.User]);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task ChangeUserRoles_ShouldReturnBadRequest_WhenUserTriesToRemoveAdministratorRoleFromThemselves()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.ChangeUserRolesAsync(Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(new OperationResult<UserDto>(true, new UserDto()));

        ActionResult<UserDto> response = await _usersController
            .ChangeUserRoles("1", [DefaultRoles.User]);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserRoles_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.ChangeUserRolesAsync(Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(new OperationResult<UserDto>(false, "Failed!"));

        ActionResult<UserDto> response = await _usersController
            .ChangeUserRoles("1", [DefaultRoles.Administrator, DefaultRoles.User]);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserRoles_ShouldReturnBadRequest_WhenCantGetUserId()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.ChangeUserRolesAsync(Arg.Any<string>(), Arg.Any<string[]>())
            .Returns(new OperationResult<UserDto>(false, "Failed!"));

        ActionResult<UserDto> response = await _usersController
            .ChangeUserRoles("1", [DefaultRoles.Administrator, DefaultRoles.User]);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteUser_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.DeleteUserAsync(Arg.Any<string>()).Returns(new OperationResult(true));

        ActionResult response = await _usersController.DeleteUser("2");
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenUserTriesToDeleteThemselves()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.DeleteUserAsync(Arg.Any<string>()).Returns(new OperationResult(true));

        ActionResult response = await _usersController.DeleteUser("1");
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.DeleteUserAsync(Arg.Any<string>()).Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _usersController.DeleteUser("2");
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenCantGetUserId()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!,
            Roles = [DefaultRoles.Administrator]
        };
        ControllerHelper.MockUserIdentity(userDto, _usersController);
        _userService.DeleteUserAsync(Arg.Any<string>()).Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _usersController.DeleteUser("2");
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task GetAllRoles_ShouldReturnOkResult()
    {
        ActionResult<IEnumerable<string>> response = await _usersController.GetAllRoles();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
}
