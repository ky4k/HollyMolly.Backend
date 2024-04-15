using HM.BLL.Interfaces;
using HM.BLL.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace HM.BLL.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _useSsl;
    private readonly string _userName;
    private readonly string _userPassword;

    public EmailSender(
        IConfiguration configuration,
        ILogger<EmailSender> logger
        )
    {
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
    }

    public async Task<OperationResult> SendEmailAsync(IUserMailInfo recipient, string subject,
        string htmlText, CancellationToken cancellationToken)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Holly Molly", "backteamchellenge@gmail.com"));
        email.To.Add(new MailboxAddress($"{recipient.FirstName} {recipient.LastName})",
            recipient.Email));

        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlText
        };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, _useSsl, cancellationToken);
            await smtp.AuthenticateAsync(_userName, _userPassword, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
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
        if (value == null)
        {
            _logger.LogError("Cannot get the value of the {key} from the environment or " +
                "the configuration. Mail sending may not work correctly.", key);
        }
        return value ?? "";
    }
}
