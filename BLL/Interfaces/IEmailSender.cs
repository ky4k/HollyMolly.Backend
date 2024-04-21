using HM.BLL.Models.Common;
using HM.BLL.Models.Users;

namespace HM.BLL.Interfaces;

public interface IEmailSender
{
    Task<OperationResult> SendEmailAsync(UserMailInfo recipient, string subject, string htmlText, CancellationToken cancellationToken);
}
