using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;

namespace WebAPI.IntegrationTests.Mocks;

public class MockEmailSender : IEmailSender
{
    public Task<OperationResult> SendEmailAsync(UserMailInfo recipient, string subject, string htmlText, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult(true));
    }
}
