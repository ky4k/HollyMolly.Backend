using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HM.BLL.UnitTests.Services;

public class AccountServiceTests
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly HmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfigurationHelper _configurationHelper;
    private readonly ILogger<AccountService> _logger;
    private readonly AccountService _accountService;
    public AccountServiceTests()
    {
        _jwtSecurityTokenHandler = Substitute.ForPartsOf<JwtSecurityTokenHandler>();
        _context = ServiceHelper.GetTestDbContext();
        _userManager = ServiceHelper.GetUserManager(_context);
        _configurationHelper = Substitute.For<IConfigurationHelper>();
        _logger = Substitute.For<ILogger<AccountService>>();
        _accountService = new AccountService(_context, _userManager, _jwtSecurityTokenHandler,
            _configurationHelper, _logger);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldRegisterNewUser()
    {
        _userManager.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<User>())
            .Returns(["Registered user"]);
        RegistrationRequest request = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        OperationResult<RegistrationResponse> result = await _accountService.RegisterUserAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFalseResult_IfUserAlreadyExist()
    {
        await SeedDbContextAsync();
        RegistrationRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<RegistrationResponse> result = await _accountService.RegisterUserAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFalseResult_WhenEmailUsedByOidcUser()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        RegistrationRequest request = new()
        {
            Email = "user2@example.com",
            Password = "password"
        };

        OperationResult<RegistrationResponse> result = await _accountService.RegisterUserAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFalseResult_IfUserManagerThrowsError()
    {
        await SeedDbContextAsync();
        RegistrationRequest request = new()
        {
            Email = "test@example.com",
            Password = "password"
        };

        OperationResult<RegistrationResponse> result = await _accountService.RegisterUserAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RegisterOidcUserAsync_ShouldRegisterNewUser_WhenTheyDoesNotAlreadyExist()
    {
        _userManager.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        OperationResult result = await _accountService.RegisterOidcUserAsync("testUser@example.com");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RegisterOidcUserAsync_ShouldReturnTrueResult_WhenUserAlreadyRegistered()
    {
        await SeedDbContextAsync();
        _userManager.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        OperationResult result = await _accountService.RegisterOidcUserAsync("user1@example.com");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RegisterOidcUserAsync_ShouldReturnFalseResult_WhenNoEmailIsProvided()
    {
        OperationResult result = await _accountService.RegisterOidcUserAsync(null!);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RegisterOidcUserAsync_ShouldReturnFalseResult_WhenUserManagerThrowsError()
    {
        OperationResult result = await _accountService.RegisterOidcUserAsync("user1@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetConfirmationEmailKeyAsync_ShouldGenerateEmailKey_WhenUserExist()
    {
        await SeedDbContextAsync();
        _userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<User>())
            .Returns("uniqueToken");

        OperationResult<ConfirmationEmailDto> result = await _accountService
            .GetConfirmationEmailKeyAsync("1");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Payload.Token);
    }
    [Fact]
    public async Task GetConfirmationEmailKeyAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<User>())
            .Returns("uniqueToken");

        OperationResult<ConfirmationEmailDto> result = await _accountService
            .GetConfirmationEmailKeyAsync("999");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetConfirmationEmailKeyAsync_ShouldReturnFalseResult_WhenUserManagerThrowsError()
    {
        await SeedDbContextAsync();

        OperationResult<ConfirmationEmailDto> result = await _accountService
            .GetConfirmationEmailKeyAsync("1");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ConfirmEmailAsync_ShouldConfirmUserEmail()
    {
        await SeedDbContextAsync();
        _userManager.ConfirmEmailAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        OperationResult result = await _accountService.ConfirmEmailAsync("1", "confirmationToken");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.ConfirmEmailAsync("999", "confirmationToken");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldLogUserIn_WhenCredentialsCorrect()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.AccessToken);
    }
    [Fact]
    public async Task LoginAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        LoginRequest request = new()
        {
            Email = "nonExistingUser@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldReturnFalseResult_WhenPasswordDoesNotMatch()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(false);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldUseDefaultValuesCreatingToken_WhenConfigurationIsNull()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue(Arg.Any<string>()).Returns((string?)null);
        var service = new AccountService(_context, _userManager, _jwtSecurityTokenHandler, _configurationHelper, _logger);
        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldUseDefaultValuesCreatingToken_WhenNoAdditionalConfigurationProvided()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:ExpirationTimeInMinutes").Returns("UseDefault");
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);

        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldUseEnvironmentVariablesCreatingToken_WhenProvided()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns("LongEnoughTestSecurityKeyForTheApplication");
        _configurationHelper.GetConfigurationValue("JwtSettings:ExpirationTimeInMinutes").Returns("UseDefault");

        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task LoginAsync_ShouldUseConfigurationSettingsCreatingToken_WhenProvided()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:ExpirationTimeInMinutes").Returns("30");
        _configurationHelper.GetConfigurationValue("JwtSettings:Issuer").Returns("TestIssuer");
        _configurationHelper.GetConfigurationValue("JwtSettings:Audience").Returns("localhost");
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns("LongEnoughTestSecurityKeyForTheApplication");
        _configurationHelper.GetConfigurationValue("JwtSettings:RefreshTokenValidForMinutes").Returns("60");
        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        _ = _configurationHelper.Received().GetConfigurationValue("JwtSettings:ExpirationTimeInMinutes");
        _ = _configurationHelper.Received().GetConfigurationValue("JwtSettings:Issuer");
        _ = _configurationHelper.Received().GetConfigurationValue("JwtSettings:Audience");
        _ = _configurationHelper.Received().GetConfigurationValue("JwtSettings:SecurityKey");
    }
    [Fact]
    public async Task LoginAsync_ShouldReturnFalseResult_WhenConfigurationIsInvalid()
    {
        await SeedDbContextAsync();
        _userManager.CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns("TooShort");

        LoginRequest request = new()
        {
            Email = "user1@example.com",
            Password = "password"
        };

        OperationResult<LoginResponse> result = await _accountService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetOidcTokenAsync_ShouldReturnToken_WhenEmailIsCorrect()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _accountService.GetOidcTokenAsync("user1@example.com");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task GetOidcTokenAsync_ShouldWriteTokenToTheDatabase_WhenEmailIsCorrect()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _accountService.GetOidcTokenAsync("user1@example.com");
        User? user = _context.Users.FirstOrDefault(u => u.Email == "user1@example.com");

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.NotNull(user);
        Assert.Equal(user.OidcToken, result.Payload);
    }
    [Fact]
    public async Task GetOidcTokenAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult<string> result = await _accountService.GetOidcTokenAsync("nonExistingUser@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetOidcTokenAsync_ShouldReturnFalseResult_WhenUserManagerThrowsError()
    {
        await SeedDbContextAsync();
        _userManager.UpdateAsync(Arg.Any<User>()).ThrowsAsync<InvalidOperationException>();

        OperationResult<string> result = await _accountService.GetOidcTokenAsync("user1@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task LoginOidcUserAsync_ShouldLoginUser_WhenOidcTokenIsCorrect()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);

        OperationResult<LoginResponse> result = await _accountService.LoginOidcUserAsync("TestToken");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.AccessToken);
    }
    [Fact]
    public async Task LoginOidcUserAsync_ShouldReturnFalseResult_WhenTokenIsIncorrect()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);

        OperationResult<LoginResponse> result = await _accountService.LoginOidcUserAsync("InvalidToken");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task RefreshTokenAsync_ShouldRefreshAccessToken_WhenProvidedCorrectTokensToExchange()
    {
        await _context.Tokens.AddRangeAsync(Tokens);
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        TokensDto tokensDto = new()
        {
            AccessToken = Tokens[0].AccessToken,
            RefreshToken = Tokens[0].RefreshToken
        };

        OperationResult<LoginResponse> result = await _accountService.RefreshTokenAsync(tokensDto);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFalseResult_WhenTokensToExchangeAreNotAValidPair()
    {
        await _context.Tokens.AddRangeAsync(Tokens);
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        TokensDto tokensDto = new()
        {
            AccessToken = Tokens[1].AccessToken,
            RefreshToken = Tokens[0].RefreshToken
        };

        OperationResult<LoginResponse> result = await _accountService.RefreshTokenAsync(tokensDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFalseResult_WhenRefreshTokenHasExpired()
    {
        await _context.Tokens.AddRangeAsync(Tokens);
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        TokensDto tokensDto = new()
        {
            AccessToken = Tokens[2].AccessToken,
            RefreshToken = Tokens[2].RefreshToken
        };

        OperationResult<LoginResponse> result = await _accountService.RefreshTokenAsync(tokensDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await _context.Tokens.AddRangeAsync(Tokens);
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        TokensDto tokensDto = new()
        {
            AccessToken = Tokens[1].AccessToken,
            RefreshToken = Tokens[1].RefreshToken
        };

        OperationResult<LoginResponse> result = await _accountService.RefreshTokenAsync(tokensDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RefreshTokenAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await dbContextMock.Tokens.AddRangeAsync(Tokens);
        await SeedDbContextAsync(dbContextMock);
        UserManager<User> userManager = ServiceHelper.GetUserManager(dbContextMock);
        userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _configurationHelper.GetConfigurationValue("JwtSettings:SecurityKey").Returns((string?)null);
        TokensDto tokensDto = new()
        {
            AccessToken = Tokens[0].AccessToken,
            RefreshToken = Tokens[0].RefreshToken
        };
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new AccountService(dbContextMock, userManager, _jwtSecurityTokenHandler,
            _configurationHelper, _logger);

        OperationResult<LoginResponse> result = await service.RefreshTokenAsync(tokensDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task InvalidateAllPreviousTokensAsync_ShouldSetInvalidateTokenBeforeColumnForTheUser()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.InvalidateAllPreviousTokensAsync("1");
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == "1");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(user?.InvalidateTokenBefore);
    }
    [Fact]
    public async Task InvalidateAllPreviousTokensAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.InvalidateAllPreviousTokensAsync("999");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateUserProfileAsync_ShouldUpdateUserProfile()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        ProfileUpdateDto profile = new()
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            City = "UpdatedCity",
            DeliveryAddress = "UpdatedAddress",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };

        OperationResult<UserDto> result = await _accountService.UpdateUserProfileAsync("1", profile);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equivalent(profile, result.Payload);
    }
    [Fact]
    public async Task UpdateUserProfileAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        ProfileUpdateDto profile = new()
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            City = "UpdatedCity",
            DeliveryAddress = "UpdatedAddress",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };

        OperationResult<UserDto> result = await _accountService.UpdateUserProfileAsync("999", profile);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);

    }
    [Fact]
    public async Task UpdateUserProfileAsync_ShouldReturnFalseResult_WhenUserHasNotBeenUpdated()
    {
        await SeedDbContextAsync();
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Registered user"]);
        _userManager.UpdateAsync(Arg.Any<User>()).Returns(IdentityResult.Failed());
        ProfileUpdateDto profile = new()
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            City = "UpdatedCity",
            DeliveryAddress = "UpdatedAddress",
            DateOfBirth = new DateOnly(2000, 1, 1),
            PhoneNumber = "1234567890"
        };

        OperationResult<UserDto> result = await _accountService.UpdateUserProfileAsync("1", profile);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UpdateEmailAsync_ShouldUpdateUserNameAndEmail()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.UpdateEmailAsync("1", "updated@example.com");
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == "1");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(user);
        Assert.Multiple(
            () => Assert.Equal("updated@example.com", user.Email),
            () => Assert.Equal("updated@example.com", user.UserName),
            () => Assert.False(user.EmailConfirmed),
            () => Assert.NotNull(user.InvalidateTokenBefore)
        );
    }
    [Fact]
    public async Task UpdateEmailAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.UpdateEmailAsync("999", "updated@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateEmailAsync_ShouldReturnFalseResult_WhenUserWasAuthorizedThroughExternalProvider()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.UpdateEmailAsync("2", "updated@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateEmailAsync_ShouldReturnFalseResult_WhenUserEmailIsUsedByAnotherUser()
    {
        await SeedDbContextAsync();

        OperationResult result = await _accountService.UpdateEmailAsync("1", "user2@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnTrueResult_WhenOldPasswordIsCorrect()
    {
        await SeedDbContextAsync();
        _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).Returns("ResetToken");
        ChangePasswordDto passwords = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .ChangePasswordAsync("1", passwords);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.Token);
    }
    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFalseResult_WhenOldPasswordIsIncorrect()
    {
        await SeedDbContextAsync();
        _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed());
        _userManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).Returns("ResetToken");
        ChangePasswordDto passwords = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .ChangePasswordAsync("1", passwords);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed());
        _userManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).Returns("ResetToken");
        ChangePasswordDto passwords = new()
        {
            OldPassword = "oldPassword",
            NewPassword = "newPassword"
        };

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .ChangePasswordAsync("999", passwords);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task CreatePasswordResetKeyAsync_ShouldReturnTrueResult_WhenEmailIsCorrect()
    {
        await SeedDbContextAsync();
        _userManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).Returns("ResetToken");

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .CreatePasswordResetKeyAsync("user1@example.com");

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Payload?.Token);
    }
    [Fact]
    public async Task CreatePasswordResetKeyAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).Returns("ResetToken");

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .CreatePasswordResetKeyAsync("nonExistingUser@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task CreatePasswordResetKeyAsync_ShouldReturnFalseResult_WhenResetTokenWasNotGenerated()
    {
        await SeedDbContextAsync();

        OperationResult<ResetPasswordTokenDto> result = await _accountService
            .CreatePasswordResetKeyAsync("user1@example.com");

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnTrueResult_WhenPasswordHasBeenChanged()
    {
        await SeedDbContextAsync();
        _userManager.VerifyUserTokenAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "ResetToken",
            NewPassword = "NewPassword"
        };

        OperationResult<UserDto> result = await _accountService.ResetPasswordAsync("1", resetPasswordDto);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalseResult_WhenUserDoesNotExist()
    {
        await SeedDbContextAsync();
        _userManager.VerifyUserTokenAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "ResetToken",
            NewPassword = "NewPassword"
        };

        OperationResult<UserDto> result = await _accountService.ResetPasswordAsync("999", resetPasswordDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalseResult_WhenTokenIsInvalid()
    {
        await SeedDbContextAsync();
        _userManager.VerifyUserTokenAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "ResetToken",
            NewPassword = "NewPassword"
        };

        OperationResult<UserDto> result = await _accountService.ResetPasswordAsync("1", resetPasswordDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalseResult_WhenPasswordHasNotBeenChanged()
    {
        await SeedDbContextAsync();
        _userManager.VerifyUserTokenAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        _userManager.RemovePasswordAsync(Arg.Any<User>()).ThrowsAsync<InvalidOperationException>();
        ResetPasswordDto resetPasswordDto = new()
        {
            ResetToken = "ResetToken",
            NewPassword = "NewPassword"
        };

        OperationResult<UserDto> result = await _accountService.ResetPasswordAsync("1", resetPasswordDto);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
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
            NormalizedUserName = "USER1@EXAMPLE.COM",
            Email = "user1@example.com",
            NormalizedEmail = "USER1@EXAMPLE.COM",
            FirstName = "First",
            LastName = "User"
        },
        new()
        {
            Id = "2",
            UserName = "user2@example.com",
            NormalizedUserName = "USER2@EXAMPLE.COM",
            Email = "user2@example.com",
            NormalizedEmail = "USER2@EXAMPLE.COM",
            IsOidcUser = true,
            OidcToken = "TestToken",
            FirstName = "Second",
            LastName = "User"
        },
        new()
        {
            Id = "3",
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            FirstName = "Default",
            LastName = "Administrator"
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

    private List<TokenRecord> Tokens =>
    [
        new()
        {
            Id = 1,
            UserName = "user1@example.com",
            AccessToken = "accessToken1",
            RefreshToken = GetRefreshToken("user1@example.com", new DateTime(2032, 12, 31, 12, 0, 0, DateTimeKind.Utc))
        },
        new()
        {
            Id = 2,
            UserName = "user99@example.com",
            AccessToken = "accessToken2",
            RefreshToken = GetRefreshToken("user99@example.com", new DateTime(2032, 12, 31, 12, 0, 0, DateTimeKind.Utc))
        },
        new()
        {
            Id = 3,
            UserName = "user1@example.com",
            AccessToken = "accessToken3",
            RefreshToken = GetRefreshToken("user1@example.com", new DateTime(2000, 12, 31, 12, 0, 0, DateTimeKind.Utc))
        },
    ];

    private string GetRefreshToken(string userName, DateTime expirationDate)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "TokenId"),
            new(ClaimTypes.Name, userName)
        };

        byte[] key = Encoding.UTF8.GetBytes("defaultKey_that_is_32_characters");
        var secret = new SymmetricSecurityKey(key);
        JwtSecurityToken refreshToken = new(
            issuer: "HollyMolly",
            audience: "*",
            claims: claims,
            expires: expirationDate,
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
        return _jwtSecurityTokenHandler.WriteToken(refreshToken);
    }
}
