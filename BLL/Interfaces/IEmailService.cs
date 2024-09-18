using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Supports;
using HM.BLL.Models.Users;

namespace HM.BLL.Interfaces;

public interface IEmailService
{
    Task<OperationResult> SendRegistrationResultEmailAsync(string email,
        ConfirmationEmailDto confirmationEmail, CancellationToken cancellationToken);
    Task<OperationResult> SendForgetPasswordEmailAsync(string email,
        ResetPasswordTokenDto resetPassword, CancellationToken cancellationToken);
    Task<OperationResult> SendPasswordChangedEmailAsync(string email,
        ResetPasswordTokenDto resetPassword, CancellationToken cancellationToken);
    Task<OperationResult> SendEmailChangedEmailAsync(string email, CancellationToken cancellationToken);
    Task<OperationResult> SendOrderCreatedEmailAsync(OrderDto order, CancellationToken cancellationToken);
    Task<OperationResult> SendOrderStatusUpdatedEmailAsync(OrderDto order, CancellationToken cancellationToken);
    Task<OperationResult> SendNewsEmailAsync(IEnumerable<NewsSubscriptionDto> subscriptions,
        string subject, string textHtml, CancellationToken cancellationToken);
    Task<OperationResult> SendSupportEmailAsync(SupportCreateDto supportDto, CancellationToken cancellationToken);
    Task<OperationResult> SendInternetDocumentCreatedEmailAsync(OrderDto order, string documentNumber, CancellationToken cancellationToken);
}
