using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class NewsSubscriptionService(
    HmDbContext context,
    ILogger<NewsSubscriptionService> logger
    ) : INewsSubscriptionService
{
    public async Task<IEnumerable<NewsSubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken cancellationToken)
    {
        return await context.NewsSubscriptions
            .Select(s => new NewsSubscriptionDto()
            {
                Email = s.Email,
                RemoveToken = s.RemoveToken
            }).ToListAsync(cancellationToken);
    }

    public async Task<OperationResult> AddSubscriptionAsync(NewsSubscriptionCreateDto subscriptionDto, CancellationToken cancellationToken)
    {
        if (await context.NewsSubscriptions.AnyAsync(s => s.Email == subscriptionDto.Email, cancellationToken))
        {
            return new OperationResult(true, "Subscription has already existed.");
        }
        NewsSubscription newsSubscription = new()
        {
            Email = subscriptionDto.Email,
            RemoveToken = Guid.NewGuid().ToString()
        };
        try
        {
            await context.NewsSubscriptions.AddAsync(newsSubscription, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true, "Subscription has been added.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding news subscription.");
            return new OperationResult(false, "Subscription has not been added.");
        }
    }

    public async Task<OperationResult> RemoveSubscriptionAsync(string removeToken, CancellationToken cancellationToken)
    {
        NewsSubscription? newsSubscription = await context.NewsSubscriptions
            .FirstOrDefaultAsync(ns => ns.RemoveToken == removeToken, cancellationToken);

        if (newsSubscription == null)
        {
            return new OperationResult(true, "Subscription has not already exist");
        }
        try
        {
            context.NewsSubscriptions.Remove(newsSubscription);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true, "Subscription has been deleted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error has occurred while deleting subscription");
            return new OperationResult(false, "Subscription has not been deleted");
        }
    }
}
