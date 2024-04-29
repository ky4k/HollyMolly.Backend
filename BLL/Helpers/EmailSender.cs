using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace HM.BLL.Helpers;

public class EmailSender : IEmailSender
{
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly HmDbContext _context;
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _useSsl;
    private readonly string _userName;
    private readonly string _userPassword;
    private readonly string _sender;

    public EmailSender(
        ISmtpClientFactory smtpClientFactory,
        HmDbContext context,
        IConfiguration configuration,
        ILogger<EmailSender> logger
        )
    {
        _smtpClientFactory = smtpClientFactory;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _smtpHost = ReadConfiguration("SMTPServer:Host");
        if (!int.TryParse(ReadConfiguration("SMTPServer:Port"), out _smtpPort))
        {
            _smtpPort = 587;
        }
        if (!bool.TryParse(ReadConfiguration("SMTPServer:UseSSL"), out _useSsl))
        {
            _useSsl = false;
        }
        _userName = ReadConfiguration("SMTPServer:User");
        _userPassword = ReadConfiguration("SMTPServer:Password");
        _sender = ReadConfiguration("SMTPServer:Sender");
    }

    public async Task<OperationResult> SendEmailAsync(UserMailInfo recipient, string subject,
        string htmlText, CancellationToken cancellationToken)
    {
        if (await ExceededTodayEmailLimitAsync(cancellationToken))
        {
            return new OperationResult(false, "Email was not sent. You reached today's email limit.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Holly Molly", _sender));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlText
        };

        try
        {
            using var smtp = _smtpClientFactory.CreateClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, _useSsl, cancellationToken);
            await smtp.AuthenticateAsync(_userName, _userPassword, cancellationToken);

            await _context.EmailLogs.AddAsync(new EmailLog()
            {
                RecipientEmail = recipient.Email,
                Subject = email.Subject,
                SendAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            email.To.Add(new MailboxAddress($"{recipient.FirstName} {recipient.LastName}", recipient.Email));
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Email has been sent: {@emailTo}, \n{emailSubject}, \n{emailBody}", email.To, email.Subject, email.HtmlBody);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending an email");
            return new OperationResult(false, "The email was not send");
        }
    }

    private string ReadConfiguration(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        value ??= _configuration.GetValue<string>(key);
        if (string.IsNullOrEmpty(value))
        {
            _logger.LogError("Cannot get the value of the {key} from the environment or " +
                "the configuration. Mail sending may not work correctly.", key);
        }
        return value!;
    }

    private async Task<bool> ExceededTodayEmailLimitAsync(CancellationToken cancellationToken)
    {
        int sentEmailCount = await _context.EmailLogs
            .CountAsync(el => el.SendAt > DateTimeOffset.UtcNow.AddDays(-1), cancellationToken);
        return sentEmailCount > 100;
    }
}
