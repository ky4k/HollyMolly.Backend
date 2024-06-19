using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace HM.BLL.UnitTests.Services;

public class UserServiceTests
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly HmDbContext _context;
    private readonly UserService _userService;
    public UserServiceTests()
    {
        _context = ServiceHelper.GetTestDbContext();
        _userManager = ServiceHelper.GetUserManager(_context);
        _roleManager = ServiceHelper.GetRoleManager(_context);
        _userService = new UserService(_userManager, _roleManager, _context);
    }
    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        await SeedDbContextAsync();

        IEnumerable<UserDto> users = await _userService.GetUsersAsync(CancellationToken.None);

        Assert.Equal(3, users.Count());
    }
    [Fact]
    public async Task GetUsersAsync_ShouldReturnRolesForEveryUser()
    {
        await SeedDbContextAsync();

        IEnumerable<UserDto> users = await _userService.GetUsersAsync(CancellationToken.None);

        Assert.Equal(3, users.Count());
        Assert.Equal(3, users.Count(u => u.Roles.Contains("Registered user")));
        Assert.Equal(1, users.Count(u => u.Roles.Contains("Administrator")));
    }
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);

        UserDto? user = await _userService.GetUserByIdAsync("1");

        Assert.NotNull(user);
        Assert.Equal("user1@example.com", user.Email);
    }
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);

        UserDto? user = await _userService.GetUserByIdAsync("999");

        Assert.Null(user);
    }
    [Fact]
    public async Task ChangeUserRolesAsync_ShouldCallUserManagerToRemoveAndAddRoles()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _userManager.RemoveFromRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Success);
        _userManager.AddToRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Success);

        OperationResult<UserDto> result = await _userService.ChangeUserRolesAsync("1", ["Manager"]);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        await _userManager.Received().RemoveFromRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>());
        await _userManager.Received().AddToRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>());
    }
    [Fact]
    public async Task ChangeUserRolesAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<UserDto> result = await _userService.ChangeUserRolesAsync("999", ["Manager"]);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ChangeUserRolesAsync_ShouldReturnFalseResult_WhenRoleDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<UserDto> result = await _userService.ChangeUserRolesAsync("1", ["NotExistingRole"]);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ChangeUserRolesAsync_ShouldReturnFalseResult_WhenUserManagerReturnErrorResult()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _userManager.RemoveFromRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Failed());
        _userManager.AddToRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>())
            .Returns(IdentityResult.Failed());

        OperationResult<UserDto> result = await _userService.ChangeUserRolesAsync("1", ["Manager"]);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        await _userManager.Received().RemoveFromRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>());
        await _userManager.Received().AddToRolesAsync(Arg.Any<User>(), Arg.Any<IEnumerable<string>>());
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldCallDeleteOnUserManager()
    {
        await SeedDbContextAsync();
        _userManager.DeleteAsync(Arg.Any<User>()).Returns(IdentityResult.Success);

        OperationResult result = await _userService.DeleteUserAsync("1");

        Assert.True(result.Succeeded);
        await _userManager.Received().DeleteAsync(Arg.Any<User>());
    }
    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalseResult_WhenUserManagerReturnError()
    {
        await SeedDbContextAsync();
        _userManager.DeleteAsync(Arg.Any<User>()).Returns(IdentityResult.Failed());

        OperationResult result = await _userService.DeleteUserAsync("1");

        Assert.False(result.Succeeded);
        await _userManager.Received().DeleteAsync(Arg.Any<User>());
    }
    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _userService.DeleteUserAsync("999");

        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnAllRoles()
    {
        await SeedDbContextAsync();

        IEnumerable<string> result = await _userService.GetAllRolesAsync();

        Assert.Equal(3, result.Count());
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.Roles.AddRangeAsync(Roles);
        await context.Users.AddRangeAsync(Users);
        await context.UserRoles.AddRangeAsync(UserRoles);
        await context.SaveChangesAsync();
    }

    private static List<Role> Roles =>
    [
        new()
        {
            Id = "1",
            Name = "Administrator",
        },
        new()
        {
            Id = "2",
            Name = "Manager",
        },
        new()
        {
            Id = "3",
            Name = "Registered user"
        }
    ];
    private static List<User> Users =>
    [
        new()
        {
            Id = "1",
            UserName = "user1@example.com",
            Email = "user1@example.com",
            Profiles =
            [
                new()
                {
                    FirstName = "First",
                    LastName = "User"
                }
            ]
        },
        new()
        {
            Id = "2",
            UserName = "user2@example.com",
            Email = "user2@example.com",
            Profiles =
            [
                new()
                {
                    FirstName = "Second",
                    LastName = "User"
                }
            ]
        },
        new()
        {
            Id = "3",
            UserName = "admin@example.com",
            Email = "admin@example.com",
            Profiles =
            [
                new()
                {
                    FirstName = "Default",
                    LastName = "Administrator"
                }
            ]
        }
    ];

    private static List<IdentityUserRole<string>> UserRoles =>
    [
        new()
        {
            RoleId = "3",
            UserId = "1"
        },
        new()
        {
            RoleId = "3",
            UserId = "2"
        },
        new()
        {
            RoleId = "1",
            UserId = "3"
        },
        new()
        {
            RoleId = "3",
            UserId = "3"
        }
    ];
}
