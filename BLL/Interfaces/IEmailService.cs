using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IEmailService
{
    Task<OperationResult> SendRegistrationResultEmailAsync();
    Task<OperationResult> SendForgetPasswordEmailAsync();
    Task<OperationResult> SendOrderCreatedEmailAsync();
    Task<OperationResult> SendOrderStatusUpdatedEmailAsync();
    Task<OperationResult> SendNewsEmailAsync();
}
