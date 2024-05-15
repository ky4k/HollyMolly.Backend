using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace HM.BLL.Helpers;

public class EmailSender : IEmailSender
{
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly HmDbContext _context;
    private readonly ILogger<EmailSender> _logger;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _useSsl;
    private readonly string? _userName;
    private readonly string? _userPassword;
    private readonly string? _sender;

    public EmailSender(
        ISmtpClientFactory smtpClientFactory,
        HmDbContext context,
        IConfigurationHelper configurationHelper,
        ILogger<EmailSender> logger
        )
    {
        _smtpClientFactory = smtpClientFactory;
        _context = context;
        _logger = logger;
        _smtpHost = configurationHelper.GetConfigurationValue("SMTPServer:Host");
        if (!int.TryParse(configurationHelper.GetConfigurationValue("SMTPServer:Port"), out _smtpPort))
        {
            _smtpPort = 587;
        }
        if (!bool.TryParse(configurationHelper.GetConfigurationValue("SMTPServer:UseSSL"), out _useSsl))
        {
            _useSsl = false;
        }
        _userName = configurationHelper.GetConfigurationValue("SMTPServer:User");
        _userPassword = configurationHelper.GetConfigurationValue("SMTPServer:Password");
        _sender = configurationHelper.GetConfigurationValue("SMTPServer:Sender");
    }

    public async Task<OperationResult> SendEmailAsync(UserMailInfo recipient, string subject,
        string htmlText, CancellationToken cancellationToken)
    {
        if (await ExceededTodayEmailLimitAsync(cancellationToken))
        {
            return new OperationResult(false, "Email was not sent. You reached today's email limit.");
        }
        MimeMessage emailMessage = PrepareEmailMessage(recipient, subject, htmlText);
        try
        {
            using var smtp = _smtpClientFactory.CreateClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, _useSsl, cancellationToken);
            await smtp.AuthenticateAsync(_userName, _userPassword, cancellationToken);
            await smtp.SendAsync(emailMessage, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            await LogAsync(recipient.Email, emailMessage, cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending an email");
            return new OperationResult(false, "The email was not send");
        }
    }

    private async Task<bool> ExceededTodayEmailLimitAsync(CancellationToken cancellationToken)
    {
        int sentEmailCount = await _context.EmailLogs
            .CountAsync(el => el.SendAt > DateTimeOffset.UtcNow.AddDays(-1), cancellationToken);
        return sentEmailCount > 100;
    }

    private MimeMessage PrepareEmailMessage(UserMailInfo recipient, string subject, string htmlText)
    {
        MimeMessage emailMessage = new();
        emailMessage.From.Add(new MailboxAddress("Holly Molly", _sender));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlText
        };
        emailMessage.To.Add(new MailboxAddress($"{recipient.FirstName} {recipient.LastName}", recipient.Email));
        return emailMessage;
    }

    private async Task LogAsync(string recipientEmail, MimeMessage emailMessage, CancellationToken cancellationToken)
    {
        await _context.EmailLogs.AddAsync(new EmailLog()
        {
            RecipientEmail = recipientEmail,
            Subject = emailMessage.Subject,
            SendAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Email has been sent: {@EmailTo}, \n{EmailSubject}, \n{EmailBody}", emailMessage.To, emailMessage.Subject, emailMessage.HtmlBody);
    }
}
