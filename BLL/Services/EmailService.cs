using HM.BLL.Interfaces;
using HM.BLL.Models;

namespace HM.BLL.Services;

public class EmailService : IEmailService
{
    public async Task<OperationResult> SendRegistrationResultEmailAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendForgetPasswordEmailAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendPasswordChangedEmail()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendOrderCreatedEmailAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendOrderStatusUpdatedEmailAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendNewsEmailAsync()
    {
        throw new NotImplementedException();
    }
}
