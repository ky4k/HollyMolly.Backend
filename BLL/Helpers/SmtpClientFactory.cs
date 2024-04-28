using HM.BLL.Interfaces;
using MailKit.Net.Smtp;

namespace HM.BLL.Helpers;

public class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient CreateClient()
    {
        return new SmtpClient();
    }
}
