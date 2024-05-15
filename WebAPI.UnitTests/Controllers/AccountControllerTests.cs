using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
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
    private readonly IConfigurationHelper _configurationHelper;
    private readonly AccountController _accountController;
    public AccountControllerTests()
    {
        _accountService = Substitute.For<IAccountService>();
        _googleOAuthService = Substitute.For<IGoogleOAuthService>();
        _emailService = Substitute.For<IEmailService>();
        _userService = Substitute.For<IUserService>();
        _configurationHelper = Substitute.For<IConfigurationHelper>();
        _configurationHelper.GetConfigurationValue(Arg.Any<string>()).Returns((string?)null);
        _accountController = new AccountController(_accountService,
            _googleOAuthService, _emailService, _userService, _configurationHelper);
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
    public async Task ConfirmEmail_ShouldReturnNoContent_WhenEmailHasBeenConfirmed()
    {
        _accountService.ConfirmEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));

        ActionResult response = await _accountController.ConfirmEmail("1", "token");
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task ConfirmEmail_ShouldReturnBadRequest_WhenConfirmationFails()
    {
        _accountService.ConfirmEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(false, "Error message"));

        ActionResult response = await _accountController.ConfirmEmail("1", "token");
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task Login_ShouldReturnLoginResult_WhenSucceeded()
    {
        LoginResponse loginResponse = new()
        {
            UserId = "1",
            UserEmail = "user1@example.com",
            AccessToken = "access token",
            RefreshToken = "refresh token"
        };
        _accountService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns(new OperationResult<LoginResponse>(true, loginResponse));
        LoginRequest loginRequest = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        ActionResult<LoginResponse> response = await _accountController.Login(loginRequest);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenFailed()
    {
        _accountService.LoginAsync(Arg.Any<LoginRequest>())
            .Returns(new OperationResult<LoginResponse>(false, "Login failed!"));
        LoginRequest loginRequest = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        ActionResult<LoginResponse> response = await _accountController.Login(loginRequest);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public void RedirectOnGoogleOAuthServer_ShouldReturnLink()
    {
        _googleOAuthService.GenerateOAuthRequestUrl(Arg.Any<string>()).Returns("url");
        ControllerHelper.MockHost(_accountController);

        ActionResult<LinkDto> response = _accountController.RedirectOnGoogleOAuthServer();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnRedirectResult_WhenSucceeded()
    {
        ControllerHelper.MockHost(_accountController);
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("googleToken");
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("user@gmail.com");
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", "token"));

        ActionResult response = await _accountController.GetGoogleToken("code", CancellationToken.None);

        Assert.NotNull(response);
        Assert.IsType<RedirectResult>(response);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnRedirectToTheConfiguredUrl()
    {
        string testUrl = "http://test.example";
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("googleToken");
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("user@gmail.com");
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", "token"));
        _configurationHelper.GetConfigurationValue(Arg.Any<string>()).Returns(testUrl);
        var accountController = new AccountController(_accountService, _googleOAuthService,
            _emailService, _userService, _configurationHelper);
        ControllerHelper.MockHost(accountController);

        ActionResult response = await accountController.GetGoogleToken("code", CancellationToken.None);
        var result = response as RedirectResult;

        Assert.NotNull(result);
        Assert.Contains(testUrl, result.Url);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnBadRequest_WhenCannotGetToken()
    {
        ControllerHelper.MockHost(_accountController);
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("user@gmail.com");
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", "token"));

        ActionResult response = await _accountController.GetGoogleToken("code", CancellationToken.None);

        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnBadRequest_WhenCannotGetEmail()
    {
        ControllerHelper.MockHost(_accountController);
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("googleToken");
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", "token"));

        ActionResult response = await _accountController.GetGoogleToken("code", CancellationToken.None);

        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnBadRequest_WhenRegistrationFailed()
    {
        ControllerHelper.MockHost(_accountController);
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("googleToken");
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("user@gmail.com");
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(false, "Registration error!"));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", "token"));

        ActionResult response = await _accountController.GetGoogleToken("code", CancellationToken.None);

        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
    }
    [Fact]
    public async Task GetGoogleToken_ShouldReturnBadRequest_WhenTokenWasNotCreated()
    {
        ControllerHelper.MockHost(_accountController);
        _googleOAuthService.ExchangeCodeOnTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("googleToken");
        _googleOAuthService.GetUserEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("user@gmail.com");
        _accountService.RegisterOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.GetOidcTokenAsync(Arg.Any<string>())
            .Returns(new OperationResult<string>(true, "", null!));

        ActionResult response = await _accountController.GetGoogleToken("code", CancellationToken.None);

        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
    }
    [Fact]
    public async Task LoginViaGoogle_ShouldReturnOkResult_WhenSucceeded()
    {
        LoginResponse loginResponse = new()
        {
            UserId = "1",
            UserEmail = "user1@example.com",
            AccessToken = "access token",
            RefreshToken = "refresh token"
        };
        _accountService.LoginOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult<LoginResponse>(true, loginResponse));

        LoginOidcRequest loginOidcRequest = new()
        {
            Token = "token"
        };
        ActionResult<LoginResponse> response = await _accountController.LoginViaToken(loginOidcRequest);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task LoginViaGoogle_ShouldReturnOkResult_WhenFailed()
    {
        _accountService.LoginOidcUserAsync(Arg.Any<string>())
            .Returns(new OperationResult<LoginResponse>(false, "Invalid token"));

        LoginOidcRequest loginOidcRequest = new()
        {
            Token = "token"
        };
        ActionResult<LoginResponse> response = await _accountController.LoginViaToken(loginOidcRequest);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnOkResult_WhenSucceeded()
    {
        LoginResponse loginResponse = new()
        {
            UserId = "1",
            UserEmail = "user1@example.com",
            AccessToken = "access token",
            RefreshToken = "refresh token"
        };
        _accountService.RefreshTokenAsync(Arg.Any<TokensDto>())
            .Returns(new OperationResult<LoginResponse>(true, loginResponse));
        TokensDto tokens = new()
        {
            AccessToken = "AccessToken",
            RefreshToken = "RefreshToken"
        };

        ActionResult<LoginResponse> response = await _accountController.RefreshToken(tokens);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequestResult_WhenFailed()
    {
        _accountService.RefreshTokenAsync(Arg.Any<TokensDto>())
            .Returns(new OperationResult<LoginResponse>(false, "Invalid token"));
        TokensDto tokens = new()
        {
            AccessToken = "AccessToken",
            RefreshToken = "RefreshToken"
        };

        ActionResult<LoginResponse> response = await _accountController.RefreshToken(tokens);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task LogoutAllDevices_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.InvalidateAllPreviousTokensAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));

        ActionResult response = await _accountController.LogoutAllDevices();
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task LogoutAllDevices_ShouldReturnUnauthorizedResult_WhenCalledByUnauthenticatedUsers()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.InvalidateAllPreviousTokensAsync(Arg.Any<string>())
            .Returns(new OperationResult(true));

        ActionResult response = await _accountController.LogoutAllDevices();
        var result = response as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task LogoutAllDevices_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.InvalidateAllPreviousTokensAsync(Arg.Any<string>())
            .Returns(new OperationResult(false, "Logout failed!"));

        ActionResult response = await _accountController.LogoutAllDevices();
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
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
    public async Task GetProfile_ShouldReturnUnauthorized_WhenUserIsUnauthorized()
    {
        ControllerHelper.MockUserIdentity(new UserDto(), _accountController);
        ActionResult<UserDto> response = await _accountController.GetProfile();
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
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
    [Fact]
    public async Task UpdateProfile_ShouldReturnOkResult_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ProfileUpdateDto profileUpdateDto = new()
        {
            FirstName = "Updated name"
        };
        _accountService.UpdateUserProfileAsync(Arg.Any<string>(), Arg.Any<ProfileUpdateDto>())
            .Returns(new OperationResult<UserDto>(true, userDto));

        ActionResult<UserDto> response = await _accountController.UpdateProfile(profileUpdateDto);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.IsType<UserDto>(result.Value);
    }
    [Fact]
    public async Task UpdateProfile_ShouldReturnUnauthorizedResult_WhenCalledByUnauthenticatedUsers()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ProfileUpdateDto profileUpdateDto = new()
        {
            FirstName = "Updated name"
        };
        _accountService.UpdateUserProfileAsync(Arg.Any<string>(), Arg.Any<ProfileUpdateDto>())
            .Returns(new OperationResult<UserDto>(false, "Unauthorized"));

        ActionResult<UserDto> response = await _accountController.UpdateProfile(profileUpdateDto);
        var result = response.Result as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task UpdateProfile_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ProfileUpdateDto profileUpdateDto = new()
        {
            FirstName = "Updated name"
        };
        _accountService.UpdateUserProfileAsync(Arg.Any<string>(), Arg.Any<ProfileUpdateDto>())
            .Returns(new OperationResult<UserDto>(false, "Not updated!"));

        ActionResult<UserDto> response = await _accountController.UpdateProfile(profileUpdateDto);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateEmail_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateEmail_ShouldReturnUnauthorizedResult_WhenCalledByUnauthenticatedUsers()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null!
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None);
        var result = response as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task UpdateEmail_ShouldSendEmailsToOldAndNewEmail_WhenSendEmailIsTrue()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _emailService.SendEmailChangedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));
        ConfirmationEmailDto confirmationEmailDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _emailService.SendRegistrationResultEmailAsync(
            Arg.Any<string>(), Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, confirmationEmailDto));
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, confirmationEmailDto));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None, true);

        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
        await _emailService.Received()
            .SendEmailChangedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _emailService.Received()
            .SendRegistrationResultEmailAsync(Arg.Any<string>(), Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task UpdateEmail_ShouldNotSendEmailChangedEmailAsync_WhenCannotGetOldEmail()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = null!,
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _emailService.SendEmailChangedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));
        ConfirmationEmailDto confirmationEmailDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _emailService.SendRegistrationResultEmailAsync(
            Arg.Any<string>(), Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true));
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, confirmationEmailDto));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None, true);

        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateEmail_ShouldNotSendRegistrationResultEmailAsync_WhenCannotGetConfirmationToken()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(true));
        _emailService.SendEmailChangedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));
        _emailService.SendRegistrationResultEmailAsync(
            Arg.Any<string>(), Arg.Any<ConfirmationEmailDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true));
        _accountService.GetConfirmationEmailKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ConfirmationEmailDto>(true, (ConfirmationEmailDto)null!));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None, true);

        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateEmail_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.UpdateEmailAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OperationResult(false));

        ActionResult response = await _accountController
            .UpdateEmail(new EmailDto() { Email = "newEmail@example.com" }, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.ChangePasswordAsync(Arg.Any<string>(), Arg.Any<ChangePasswordDto>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ChangeUserPassword(changePasswordDto, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldReturnUnauthorizedResult_WhenCalledByUnauthenticatedUsers()
    {
        UserDto userDto = new()
        {
            Id = null!,
            Email = null
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.ChangePasswordAsync(Arg.Any<string>(), Arg.Any<ChangePasswordDto>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ChangeUserPassword(changePasswordDto, CancellationToken.None);
        var result = response as UnauthorizedResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldSendPasswordChangedEmail_WhenSendEmailIsTrue()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.ChangePasswordAsync(Arg.Any<string>(), Arg.Any<ChangePasswordDto>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        _emailService.SendPasswordChangedEmailAsync(Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ChangeUserPassword(
            changePasswordDto, CancellationToken.None, true);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
        await _emailService.Received().SendPasswordChangedEmailAsync(
            Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldNotSendEmail_WhenCannotGetUserEmail()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = null!,
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.ChangePasswordAsync(Arg.Any<string>(), Arg.Any<ChangePasswordDto>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        _emailService.SendPasswordChangedEmailAsync(Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ChangeUserPassword(
            changePasswordDto, CancellationToken.None, true);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
        await _emailService.DidNotReceive().SendPasswordChangedEmailAsync(
            Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ChangeUserPassword_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _accountController);
        _accountService.ChangePasswordAsync(Arg.Any<string>(), Arg.Any<ChangePasswordDto>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(false, "Failed!"));
        ChangePasswordDto changePasswordDto = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ChangeUserPassword(changePasswordDto, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task SendForgetPasswordEmail_ShouldReturnNoContent_WhenSucceeded()
    {
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.CreatePasswordResetKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        _emailService.SendForgetPasswordEmailAsync(Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        EmailDto emailDto = new()
        {
            Email = "user@example.com"
        };

        ActionResult response = await _accountController.SendForgetPasswordEmail(
            emailDto, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task SendForgetPasswordEmail_ShouldSendEmail_WhenSendEmailIsTrue()
    {
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "Token 1"
        };
        _accountService.CreatePasswordResetKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(true, resetPasswordTokenDto));
        _emailService.SendForgetPasswordEmailAsync(Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        EmailDto emailDto = new()
        {
            Email = "user@example.com"
        };

        ActionResult response = await _accountController.SendForgetPasswordEmail(
            emailDto, CancellationToken.None, true);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
        await _emailService.Received().SendForgetPasswordEmailAsync(
            Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task SendForgetPasswordEmail_ShouldReturnBadRequest_WhenFailed()
    {
        _accountService.CreatePasswordResetKeyAsync(Arg.Any<string>())
            .Returns(new OperationResult<ResetPasswordTokenDto>(false, "Failed!"));
        _emailService.SendForgetPasswordEmailAsync(Arg.Any<string>(), Arg.Any<ResetPasswordTokenDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        EmailDto emailDto = new()
        {
            Email = "user@example.com"
        };

        ActionResult response = await _accountController.SendForgetPasswordEmail(
            emailDto, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task ResetPassword_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        _accountService.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<ResetPasswordDto>())
            .Returns(new OperationResult<UserDto>(true, userDto));
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "Token 1",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ResetPassword("1", resetPasswordDto);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenFailed()
    {
        _accountService.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<ResetPasswordDto>())
            .Returns(new OperationResult<UserDto>(false, "Failed!"));
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "Token 1",
            NewPassword = "newPassword"
        };

        ActionResult response = await _accountController.ResetPassword("1", resetPasswordDto);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
