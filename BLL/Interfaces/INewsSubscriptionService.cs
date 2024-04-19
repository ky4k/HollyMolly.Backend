using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface INewsSubscriptionService
{
    Task<IEnumerable<NewsSubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken cancellationToken);
    Task<OperationResult> AddSubscriptionAsync(NewsSubscriptionCreateDto subscriptionDto, CancellationToken cancellationToken);
    Task<OperationResult> RemoveSubscriptionAsync(string removeToken, CancellationToken cancellationToken);
}
