using HM.BLL.Helpers;

namespace HM.BLL.UnitTests.Helpers;

public class SmtpClientFactoryTests
{
    [Fact]
    public void CreateClient_ShouldCreateSmtpClient()
    {
        SmtpClientFactory clientFactory = new();

        var client = clientFactory.CreateClient();

        Assert.NotNull(client);
        Assert.IsType<MailKit.Net.Smtp.SmtpClient>(client);
    }
}
