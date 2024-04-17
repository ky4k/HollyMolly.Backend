using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IEmailService
{
    Task<OperationResult> SendRegistrationResultEmailAsync(string email,
        ConfirmationEmailDto confirmationEmail, CancellationToken cancellationToken);
    Task<OperationResult> SendForgetPasswordEmailAsync(string email, ResetPasswordTokenDto resetPassword,
        CancellationToken cancellationToken);
    Task<OperationResult> SendPasswordChangedEmail(string email, ResetPasswordTokenDto resetPassword,
        CancellationToken cancellationToken);
    Task<OperationResult> SendOrderCreatedEmailAsync(OrderDto order, CancellationToken cancellationToken);
    Task<OperationResult> SendOrderStatusUpdatedEmailAsync(OrderDto order, CancellationToken cancellationToken);
    Task<OperationResult> SendNewsEmailAsync(IEnumerable<NewsSubscriptionDto> subscriptions,
        string subject, string textHtml, CancellationToken cancellationToken);
}
