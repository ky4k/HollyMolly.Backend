using HM.BLL.Models.Common;

namespace HM.BLL.Interfaces;

public interface ICheckoutService
{
    Task<OperationResult<string>> PayForOrderAsync(int orderId, string userId,
        string baseUrl, CancellationToken cancellationToken);
    Task<OperationResult> CheckoutSuccessAsync(string sessionId);
}
