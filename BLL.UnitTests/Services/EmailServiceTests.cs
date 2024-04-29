using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Users;
using HM.BLL.Services;
using NSubstitute;

namespace HM.BLL.UnitTests.Services;

public class EmailServiceTests
{
    private readonly IEmailSender _emailSender;
    private readonly EmailService _emailService;
    public EmailServiceTests()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _emailService = new EmailService(_emailSender);
    }

    [Fact]
    public async Task SendRegistrationResultEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        ConfirmationEmailDto confirmationEmailDto = new()
        {
            UserId = "1",
            Token = "TestToken",
        };

        OperationResult result = await _emailService.SendRegistrationResultEmailAsync(
            "test@example.com", confirmationEmailDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendRegistrationResultEmailAsync_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        ConfirmationEmailDto confirmationEmailDto = new()
        {
            UserId = "1",
            Token = "TestToken",
        };

        OperationResult result = await _emailService.SendRegistrationResultEmailAsync(
            "test@example.com", confirmationEmailDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendForgetPasswordEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "TestToken"
        };

        OperationResult result = await _emailService.SendForgetPasswordEmailAsync(
            "test@example.com", resetPasswordTokenDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendForgetPasswordEmailAsync_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "TestToken"
        };

        OperationResult result = await _emailService.SendForgetPasswordEmailAsync(
            "test@example.com", resetPasswordTokenDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendPasswordChangedEmail_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "TestToken"
        };

        OperationResult result = await _emailService.SendPasswordChangedEmail(
            "test@example.com", resetPasswordTokenDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendPasswordChangedEmail_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        ResetPasswordTokenDto resetPasswordTokenDto = new()
        {
            UserId = "1",
            Token = "TestToken"
        };

        OperationResult result = await _emailService.SendPasswordChangedEmail(
            "test@example.com", resetPasswordTokenDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendEmailChangedEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));

        OperationResult result = await _emailService.SendEmailChangedEmailAsync(
            "test@example.com", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendEmailChangedEmailAsync_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));

        OperationResult result = await _emailService.SendEmailChangedEmailAsync(
            "test@example.com", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendOrderCreatedEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        OrderDto orderDto = new()
        {
            Customer = new()
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            }
        };

        OperationResult result = await _emailService.SendOrderCreatedEmailAsync(
            orderDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendOrderCreatedEmailAsync_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        OrderDto orderDto = new()
        {
            Customer = new()
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            }
        };

        OperationResult result = await _emailService.SendOrderCreatedEmailAsync(
            orderDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendOrderStatusUpdatedEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        OrderDto orderDto = new()
        {
            Customer = new()
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            }
        };

        OperationResult result = await _emailService.SendOrderStatusUpdatedEmailAsync(
            orderDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendOrderStatusUpdatedEmailAsync_ShouldReturnFalseResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        OrderDto orderDto = new()
        {
            Customer = new()
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            }
        };

        OperationResult result = await _emailService.SendOrderStatusUpdatedEmailAsync(
            orderDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendNewsEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        List<NewsSubscriptionDto> newsSubscriptions =
        [
            new()
            {
                Email = "test@example.com",
                RemoveToken = "TestToken",
            }
        ];

        OperationResult result = await _emailService.SendNewsEmailAsync(
            newsSubscriptions, "Subject", "Test", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendNewsEmailAsync_ShouldReturnTrueResult_WhenEmailHasNotBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false));
        List<NewsSubscriptionDto> newsSubscriptions =
        [
            new()
            {
                Email = "test@example.com",
                RemoveToken = "TestToken",
            }
        ];

        OperationResult result = await _emailService.SendNewsEmailAsync(
            newsSubscriptions, "Subject", "Test", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
}
