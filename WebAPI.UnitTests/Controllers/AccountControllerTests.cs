using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class AccountControllerTests
{
    private readonly IAccountService _accountService;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly AccountController _accountController;
    public AccountControllerTests()
    {
        _accountService = Substitute.For<IAccountService>();
        _googleOAuthService = Substitute.For<IGoogleOAuthService>();
        _emailService = Substitute.For<IEmailService>();
        _userService = Substitute.For<IUserService>();
        _accountController = new AccountController(_accountService,
            _googleOAuthService, _emailService, _userService);
    }

    [Fact]
    public async Task Registration_ShouldReturnRegistrationResponse_WhenRegistrationSucceeded()
    {
        RegistrationResponse registrationResponse = new()
        {
            Id = "1",
            Email = "test@example.com",
            Roles = []
        };
        _accountService.RegisterUserAsync(Arg.Any<RegistrationRequest>())
            .Returns(new OperationResult<RegistrationResponse>(true, registrationResponse));
        RegistrationRequest registrationRequest = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        ActionResult<RegistrationResponse> response = await _accountController
            .Registration(registrationRequest, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.IsType<RegistrationResponse>(result.Value);
        Assert.NotNull((result.Value as RegistrationResponse)?.Email);
    }
    [Fact]
    public async Task Registration_ShouldReturnBadRequest_WhenRegistrationFailed()
    {
        _accountService.RegisterUserAsync(Arg.Any<RegistrationRequest>())
            .Returns(new OperationResult<RegistrationResponse>(false, "Failed"));
        RegistrationRequest registrationRequest = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        ActionResult<RegistrationResponse> response = await _accountController
            .Registration(registrationRequest, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task Registration_ShouldSendEmail_WhenSendEmailIsTrue()
    {
        RegistrationResponse registrationResponse = new()
        {
            Id = "1",
            Email = "test@example.com",
            Roles = []
        };
        _accountService.RegisterUserAsync(Arg.Any<RegistrationRequest>())
            .Returns(new OperationResult<RegistrationResponse>(true, registrationResponse));
        ConfirmationEmailDto emailDto = new()
        {
            UserId = "1",
            Token = "Token"
        };
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, emailDto));
        RegistrationRequest registrationRequest = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        await _accountController.Registration(registrationRequest, CancellationToken.None, true);

        await _emailService.Received().SendRegistrationResultEmailAsync(Arg.Any<string>(),
            Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Registration_ShouldNotSendEmail_WhenEmailConfirmationTokenWasNotCreated()
    {
        RegistrationResponse registrationResponse = new()
        {
            Id = "1",
            Email = "test@example.com",
            Roles = []
        };
        _accountService.RegisterUserAsync(Arg.Any<RegistrationRequest>())
            .Returns(new OperationResult<RegistrationResponse>(true, registrationResponse));
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, "", null!));
        RegistrationRequest registrationRequest = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        await _accountController.Registration(registrationRequest, CancellationToken.None, true);

        await _emailService.DidNotReceive().SendRegistrationResultEmailAsync(Arg.Any<string>(),
            Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task Registration_ShouldNotSendEmail_WhenCreationEmailConfirmationEmailFailed()
    {
        RegistrationResponse registrationResponse = new()
        {
            Id = "1",
            Email = "test@example.com",
            Roles = []
        };
        _accountService.RegisterUserAsync(Arg.Any<RegistrationRequest>())
            .Returns(new OperationResult<RegistrationResponse>(true, registrationResponse));
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(false));
        RegistrationRequest registrationRequest = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        await _accountController.Registration(registrationRequest, CancellationToken.None, true);

        await _emailService.DidNotReceive().SendRegistrationResultEmailAsync(Arg.Any<string>(),
            Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task GetProfile_ShouldReturnProfile_WhenUserIsAuthorized()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            FirstName = "First name",
            LastName = "Last name"
        };
        _userService.GetUserByIdAsync(Arg.Any<string>()).Returns(userDto);
        ControllerHelper.MockUserIdentity(userDto, _accountController);

        ActionResult<UserDto> response = await _accountController.GetProfile();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.IsType<UserDto>(result.Value);
        Assert.Equal(userDto.Email, (result.Value as UserDto)?.Email);
    }
    [Fact]
    public async Task GetProfile_ShouldReturn401Unauthorized_WhenUserIsUnauthorized()
    {
        ControllerHelper.MockUserIdentity(new UserDto(), _accountController);
        ActionResult<UserDto> response = await _accountController.GetProfile();
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task GetProfile_ShouldReturn404NotFound_WhenUserDoesNotExist()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            FirstName = "First name",
            LastName = "Last name"
        };
        _userService.GetUserByIdAsync(Arg.Any<string>()).Returns((UserDto?)null);
        ControllerHelper.MockUserIdentity(userDto, _accountController);

        ActionResult<UserDto> response = await _accountController.GetProfile();
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
}
