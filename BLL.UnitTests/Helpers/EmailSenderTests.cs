using HM.BLL.Helpers;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Helpers;

public class EmailSenderTests
{
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly ISmtpClient _smtpClient;
    private readonly HmDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailSender _emailSender;
    public EmailSenderTests()
    {
        _smtpClient = Substitute.For<ISmtpClient>();
        _smtpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>())).Do(x => { });
        _smtpClient.When(x => x.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>())).Do(x => { });
        _smtpClient.When(x => x.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>())).Do(x => { });
        _smtpClient.When(x => x.SendAsync(Arg.Any<MimeMessage>(), Arg.Any<CancellationToken>())).Do(x => { });
        _smtpClient.When(x => x.DisconnectAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())).Do(x => { });

        _smtpClientFactory = Substitute.For<ISmtpClientFactory>();
        _smtpClientFactory.CreateClient().Returns(_smtpClient);

        _context = ServiceHelper.GetTestDbContext();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<EmailSender>>();
        _emailSender = new EmailSender(_smtpClientFactory, _context, _configuration, _logger);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldSendEmail()
    {
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }
    [Fact]
    public async Task SendEmailAsync_ShouldLogEmail()
    {
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);
        EmailLog? logRecord = await _context.EmailLogs
            .FirstOrDefaultAsync(e => e.RecipientEmail == recipient.Email);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(logRecord);
    }
    [Fact]
    public async Task SendEmailAsync_ShouldReadConfigurations()
    {
        _configuration.GetValue<string>(Arg.Any<string>()).Returns("TestConfiguration");
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        _configuration.Received().GetValue<string>(Arg.Any<string>());
    }
    [Fact]
    public async Task SendEmailAsync_ShouldLogErrorWhenNoConfigurationsWereProvided()
    {
        _configuration.GetValue<string>(Arg.Any<string>()).Returns((string)null!);
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        _logger.ReceivedWithAnyArgs().LogError("");
    }
    [Fact]
    public async Task SendEmailAsync_ShouldHandleSmtpClientErrors()
    {
        _smtpClient.When(x => x.SendAsync(Arg.Any<MimeMessage>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException());

        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task SendEmailAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var sender = new EmailSender(_smtpClientFactory, dbContextMock, _configuration, _logger);
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await sender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldNotSendEmail_WhenTodaysEmailLimitIsExceeded()
    {
        List<EmailLog> emailLogs = [];
        for (int i = 0; i <= 100; i++)
        {
            emailLogs.Add(new EmailLog()
            {
                RecipientEmail = $"{i}@example.com",
                Subject = "Test",
                SendAt = DateTimeOffset.UtcNow
            });
        }
        await _context.EmailLogs.AddRangeAsync(emailLogs);
        await _context.SaveChangesAsync();
        UserMailInfo recipient = new()
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        OperationResult result = await _emailSender
            .SendEmailAsync(recipient, "Subject", "Text", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
}
