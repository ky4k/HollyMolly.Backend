﻿using HM.BLL.Interfaces;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
        Justification = "https://github.com/dotnet/efcore/issues/20995#issuecomment-631358780 EF Core does not translate the overload that accepts StringComparison.InvariantCultureIgnoreCase (or any other StringComparison).")]
    public async Task<OperationResult> AddSubscriptionAsync(NewsSubscriptionCreateDto subscriptionDto, CancellationToken cancellationToken)
    {
        if (await context.NewsSubscriptions.AnyAsync(s => s.Email.ToLower() == subscriptionDto.Email.ToLower(), cancellationToken))
        {
            return new OperationResult(true, "Subscription already exist.");
        }
        NewsSubscription newsSubscription = new()
        {
            Email = subscriptionDto.Email.ToLower(),
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
