using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Supports;
using HM.BLL.Models.Users;
using HM.BLL.Services;
using HM.DAL.Enums;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace HM.BLL.UnitTests.Services;

public class EmailServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly EmailService _emailService;
    public EmailServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Support:Email"].Returns("config@example.com");
        _emailSender = Substitute.For<IEmailSender>();
        _emailService = new EmailService(_emailSender, _configuration);
    }

    [Fact]
    public async Task EmailService_ShouldUseEnvironmentVariable_WhenProvided()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        string? tempVariable = Environment.GetEnvironmentVariable("Support:Email");
        Environment.SetEnvironmentVariable("Support:Email", "environment@example.com");
        var service = new EmailService(_emailSender, _configuration);
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.ProductQuestions
        };

        OperationResult result = await service.SendSupportEmailAsync(supportDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        await _emailSender.Received()
            .SendEmailAsync(Arg.Is<UserMailInfo>(u => u.Email == "environment@example.com"),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        Environment.SetEnvironmentVariable("Support:Email", tempVariable);
    }
    [Fact]
    public async Task EmailService_ShouldUseConfigurationSettings_WhenProvided()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.ProductQuestions
        };

        OperationResult result = await _emailService.SendSupportEmailAsync(supportDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        await _emailSender.Received().SendEmailAsync(Arg.Is<UserMailInfo>(u => u.Email == "config@example.com"),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task SendSupportEmailAsync_ShouldReturnTrueResult_WhenEmailHasBeenSent()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.ProductQuestions
        };

        OperationResult result = await _emailService.SendSupportEmailAsync(supportDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendSupportEmailAsync_ShouldReturnFalseResult_WhenSupportEmailIsNotConfigured()
    {
        _emailSender.SendEmailAsync(Arg.Any<UserMailInfo>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));
        _configuration["Support:Email"].Returns((string?)null);
        var service = new EmailService(_emailSender, _configuration);
        SupportCreateDto supportDto = new()
        {
            Email = "test@example.com",
            Name = "Test",
            Description = "My problem",
            Topic = SupportTopic.ProductQuestions
        };

        OperationResult result = await service.SendSupportEmailAsync(supportDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
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

        OperationResult result = await _emailService.SendPasswordChangedEmailAsync(
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

        OperationResult result = await _emailService.SendPasswordChangedEmailAsync(
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
