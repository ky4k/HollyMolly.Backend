using MailKit.Net.Smtp;

namespace HM.BLL.Interfaces;

public interface ISmtpClientFactory
{
    ISmtpClient CreateClient();
}
