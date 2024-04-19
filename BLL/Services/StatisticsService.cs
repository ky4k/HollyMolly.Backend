using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.BLL.Models.Statistics;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class StatisticsService(
    HmDbContext context,
    ILogger<StatisticsService> logger
    ) : IStatisticsService
{
    public async Task<IEnumerable<ProductStatisticDto>> GetProductStatisticsAsync(int? productId, int? categoryId,
        int? categoryGroupId, DateOnly? fromDate, DateOnly? toDate, bool yearly, bool monthly, bool daily,
        bool sortByDateFirst, bool sortByDateAsc, bool sortByProductAsc, CancellationToken cancellationToken)
    {
        IQueryable<ProductStatistics> productStatistics = context.ProductStatistics
            .Include(ps => ps.Product)
            .Include(ps => ps.ProductInstanceStatistics)
                .ThenInclude(p => p.ProductInstance);

        productStatistics = await FilterProductStatisticsAsync(productStatistics, productId, categoryId,
            categoryGroupId, fromDate, toDate, cancellationToken);

        IQueryable<IGrouping<AllTimeProductKey, ProductStatistics>> groupedResult = GroupProductStatistic(
            productStatistics, yearly, monthly, daily);

        List<ProductStatisticDto> productStatisticDtos = await GetProductStatisticDto(groupedResult)
            .ToListAsync(cancellationToken);

        IOrderedEnumerable<ProductStatisticDto> sortedProductStatistics = SortProductStatistic(
            productStatisticDtos, sortByDateFirst, sortByDateAsc, sortByProductAsc);

        return sortedProductStatistics;
    }
    
    private async Task<IQueryable<ProductStatistics>> FilterProductStatisticsAsync(
        IQueryable<ProductStatistics> productStatistics, int? productId, int? categoryId,
        int? categoryGroupId, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken)
    {
        if (productId != null)
        {
            productStatistics = productStatistics.Where(ps => ps.ProductId == productId);
        }
        else if (categoryId != null)
        {
            productStatistics = productStatistics.Where(ps => ps.Product.CategoryId == categoryId);
        }
        else if (categoryGroupId != null)
        {
            var categoriesId = await context.CategoryGroups
            .Include(cg => cg.Categories)
                .Where(cg => cg.Id == categoryGroupId)
                .SelectMany(cg => cg.Categories.Select(c => c.Id))
            .Distinct()
                .ToListAsync(cancellationToken);
            productStatistics = productStatistics.Where(ps => categoriesId.Contains(ps.Product.CategoryId));
        }

        if (fromDate != null)
        {
            productStatistics = productStatistics.Where(p => p.Year > fromDate.Value.Year
                || p.Year == fromDate.Value.Year && p.Month > fromDate.Value.Month
                || p.Year == fromDate.Value.Year && p.Month == fromDate.Value.Month && p.Day >= fromDate.Value.Day);
        }
        if (toDate != null)
        {
            productStatistics = productStatistics.Where(p => p.Year < toDate.Value.Year
                || p.Year == toDate.Value.Year && p.Month < toDate.Value.Month
                || p.Year == toDate.Value.Year && p.Month == toDate.Value.Month && p.Day <= toDate.Value.Day);
        }
        return productStatistics;
    }

    private static IQueryable<IGrouping<AllTimeProductKey, ProductStatistics>> GroupProductStatistic(
        IQueryable<ProductStatistics> productStatistics, bool yearly, bool monthly, bool daily)
    {
        IQueryable<IGrouping<AllTimeProductKey, ProductStatistics>> groupedResult;
        if (daily)
        {
            groupedResult = productStatistics.GroupBy(s => new DayProductKey
            {
                ProductId = s.ProductId,
                Year = s.Year,
                Month = s.Month,
                Day = s.Day
            });
        }
        else if (monthly)
        {
            groupedResult = productStatistics.GroupBy(s => new MonthProductKey { ProductId = s.ProductId, Year = s.Year, Month = s.Month });
        }
        else if (yearly)
        {
            groupedResult = productStatistics.GroupBy(s => new YearProductKey() { ProductId = s.ProductId, Year = s.Year });
        }
        else
        {
            groupedResult = productStatistics.GroupBy(s => new AllTimeProductKey { ProductId = s.ProductId });
        }
        return groupedResult;
    }

    private static IQueryable<ProductStatisticDto> GetProductStatisticDto(
        IQueryable<IGrouping<AllTimeProductKey, ProductStatistics>> groupedResult)
    {
        return groupedResult.Select(g => new ProductStatisticDto()
        {
            ProductId = g.Key.ProductId,
            Year = (g.Key as YearProductKey) == null ? null : (g.Key as YearProductKey)!.Year,
            Month = (g.Key as MonthProductKey) == null ? null : (g.Key as MonthProductKey)!.Month,
            Day = (g.Key as DayProductKey) == null ? null : (g.Key as DayProductKey)!.Day,
            ProductName = g.Select(ps => ps.Product.Name).First(),
            NumberViews = g.Select(ps => ps.NumberViews).DefaultIfEmpty().Sum(),
            NumberWishListAddition = g.Select(ps => ps.NumberWishlistAdditions).DefaultIfEmpty().Sum(),
            NumberReviews = g.Select(ps => ps.NumberFeedbacks).DefaultIfEmpty().Sum(),
            Rating = g.Select(ps => ps.Product).First().Rating,
            InstancesStatistics = GetProductInstanceStatisticDtos(g.SelectMany(ps => ps.ProductInstanceStatistics))
        });
    }
    private static List<ProductInstanceStatisticDto> GetProductInstanceStatisticDtos(
        IEnumerable<ProductInstanceStatistics> productInstanceStatistics)
    {
        List<ProductInstanceStatisticDto> productInstanceStatisticDtos = [];
        foreach (var instanceStatistics in productInstanceStatistics)
        {
            productInstanceStatisticDtos.Add(new ProductInstanceStatisticDto
            {
                Id = instanceStatistics.ProductInstanceId,
                Color = instanceStatistics.ProductInstance.Color,
                Size = instanceStatistics.ProductInstance.Size,
                SKU = instanceStatistics.ProductInstance.SKU,
                Material = instanceStatistics.ProductInstance.Material,
                NumberOfPurchases = instanceStatistics.NumberOfPurchases,
                TotalRevenue = instanceStatistics.TotalRevenue
            });
        }
        return productInstanceStatisticDtos;
    }

    private static IOrderedEnumerable<ProductStatisticDto> SortProductStatistic(List<ProductStatisticDto> productStatisticDtos,
        bool sortByDateFirst, bool sortByDateAsc, bool sortByProductAsc)
    {
        IOrderedEnumerable<ProductStatisticDto> orderedProductStatisticDtos;
        if (sortByDateFirst)
        {
            orderedProductStatisticDtos = sortByDateAsc
                ? productStatisticDtos.OrderBy(p => p.Year)
                    .ThenBy(p => p.Month)
                    .ThenBy(p => p.Day)
                : productStatisticDtos.OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .ThenByDescending(p => p.Day);
            orderedProductStatisticDtos = sortByProductAsc
                ? orderedProductStatisticDtos.ThenBy(p => p.ProductId)
                : orderedProductStatisticDtos.ThenByDescending(p => p.ProductId);
        }
        else
        {
            orderedProductStatisticDtos = sortByProductAsc
                ? productStatisticDtos.OrderBy(p => p.ProductId)
                : productStatisticDtos.OrderByDescending(p => p.ProductId);

            orderedProductStatisticDtos = sortByDateAsc
                ? orderedProductStatisticDtos
                    .ThenBy(p => p.Year)
                    .ThenBy(p => p.Month)
                    .ThenBy(p => p.Day)
                : orderedProductStatisticDtos
                    .ThenByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .ThenByDescending(p => p.Day);
        }
        return orderedProductStatisticDtos;
    }

    public async Task<IEnumerable<CategoryStatisticDto>> GetCategoryStatisticsAsync(
        int? categoryGroupId, int? categoryId, DateOnly? fromDate, DateOnly? toDate,
        bool yearly, bool monthly, bool daily, CancellationToken cancellationToken)
    {
        List<CategoryStatisticDto> categoryStatisticDtos = [];
        (bool succeeded, int useId, string useName) = await GetCategoryIdAndNameAsync(
            categoryGroupId, categoryId, cancellationToken);
        if(!succeeded)
        {
            return categoryStatisticDtos;
        }

        IEnumerable<ProductStatisticDto> productsStatistics = await GetProductStatisticsAsync(null, categoryId, categoryGroupId,
            fromDate, toDate, yearly, monthly, daily, true, false, true, cancellationToken);

        IEnumerable<IGrouping<YearKey, ProductStatisticDto>> groupedResult;
        if (daily)
        {
            groupedResult = productsStatistics.GroupBy(p => new DayKey { Year = p.Year, Month = p.Month, Day = p.Day });
            categoryStatisticDtos = GetCategoryStatisticDtos(groupedResult, useId, useName);
        }
        else if (monthly)
        {
            groupedResult = productsStatistics.GroupBy(p => new MonthKey { Year = p.Year, Month = p.Month });
            categoryStatisticDtos = GetCategoryStatisticDtos(groupedResult, useId, useName);
        }
        else if (yearly)
        {
            groupedResult = productsStatistics.GroupBy(p => new YearKey { Year = p.Year });
            categoryStatisticDtos = GetCategoryStatisticDtos(groupedResult, useId, useName);
        }
        else
        {
            categoryStatisticDtos.Add(new CategoryStatisticDto()
            {
                CategoryId = useId,
                CategoryName = useName,
                NumberPurchases = productsStatistics.Select(p => p.NumberPurchases).DefaultIfEmpty().Sum(),
                NumberReviews = productsStatistics.Select(p => p.NumberReviews).DefaultIfEmpty().Sum(),
                NumberProductViews = productsStatistics.Select(p => p.NumberViews).DefaultIfEmpty().Sum(),
                WishListAddition = productsStatistics.Select(p => p.NumberWishListAddition).DefaultIfEmpty().Sum(),
                TotalRevenue = productsStatistics.Select(p => p.TotalRevenue).DefaultIfEmpty().Sum(),
            });
        }
        return categoryStatisticDtos;
    }

    private async Task<(bool, int, string)> GetCategoryIdAndNameAsync(int? categoryGroupId, int? categoryId,
        CancellationToken cancellationToken)
    {
        int useId = 0;
        string useName = "";
        if (categoryGroupId != null)
        {
            var categoryGroup = await context.CategoryGroups
                .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
            if (categoryGroup == null)
            {
                return (false, 0, "");
            }
            useId = categoryGroup.Id;
            useName = categoryGroup.Name;
        }
        else if (categoryId != null)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
            if (category == null)
            {
                return (false, 0, "");
            }
            useId = category.Id;
            useName = category.Name;
        }
        else
        {
            return (false, 0, "");
        }
        return (true, useId, useName);
    }

    private static List<CategoryStatisticDto> GetCategoryStatisticDtos(
        IEnumerable<IGrouping<YearKey, ProductStatisticDto>> groupedResult, int categoryId, string categoryName)
    {
        return groupedResult.Select(g => new CategoryStatisticDto()
        {
            CategoryId = categoryId,
            CategoryName = categoryName,
            Year = g.Key.Year,
            Month = (g.Key as MonthKey)?.Month,
            Day = (g.Key as DayKey)?.Day,
            NumberPurchases = g.Select(p => p.NumberPurchases).DefaultIfEmpty().Sum(),
            NumberProductViews = g.Select(p => p.NumberViews).DefaultIfEmpty().Sum(),
            NumberReviews = g.Select(p => p.NumberReviews).DefaultIfEmpty().Sum(),
            TotalRevenue = g.Select(p => p.TotalRevenue).DefaultIfEmpty().Sum(),
            WishListAddition = g.Select(p => p.NumberWishListAddition).DefaultIfEmpty().Sum()
        }).ToList();
    }

    public async Task<IEnumerable<OrderStatisticDto>> GetOrderStatisticsAsync(
        DateOnly? fromDate, DateOnly? toDate,
        bool yearly, bool monthly, bool daily, CancellationToken cancellationToken)
    {
        IQueryable<Order> orders = context.Orders
            .Include(o => o.OrderRecords);
        if(fromDate != null)
        {
            orders.Where(o => o.OrderDate > fromDate.Value.ToDateTime(new TimeOnly(0, 0, 0)));
        }
        if(toDate != null)
        {
            orders.Where(o => o.OrderDate < toDate.Value.ToDateTime(new TimeOnly(23, 59, 59)));
        }

        List<OrderStatisticDto> orderStatisticDtos = [];
        IQueryable<IGrouping<YearKey, Order>> groupedResult;
        if (daily)
        {
            groupedResult = orders.GroupBy(o => new DayKey
            {
                Year = o.OrderDate.Date.Year,
                Month = o.OrderDate.Date.Month,
                Day = o.OrderDate.Day
            });
            orderStatisticDtos = await GetOrdersStatisticDto(groupedResult)
                .ToListAsync(cancellationToken);
        }
        else if (monthly)
        {
            groupedResult = orders.GroupBy(o => new MonthKey
            {
                Year = o.OrderDate.Date.Year,
                Month = o.OrderDate.Date.Month
            });
            orderStatisticDtos = await GetOrdersStatisticDto(groupedResult)
                .ToListAsync(cancellationToken);
        }
        else if (yearly)
        {
            groupedResult = orders.GroupBy(o => new DayKey
            {
                Year = o.OrderDate.Date.Year
            });
            orderStatisticDtos = await GetOrdersStatisticDto(groupedResult)
                .ToListAsync(cancellationToken);
        }
        else
        {
            var ordersList = await orders.ToListAsync(cancellationToken);
            orderStatisticDtos.Add(new OrderStatisticDto()
            {
                NumberOfOrders = ordersList.Select(o => o.Id).Count(),
                TotalDiscount = ordersList.SelectMany(o => o.OrderRecords.Select(or => or.Discount)).Sum(),
                TotalCost = ordersList
                    .SelectMany(o => o.OrderRecords.Select(or => or.Price * or.Quantity - or.Discount).DefaultIfEmpty()).Sum(),
            });
        }
        return orderStatisticDtos;
    }

    private static IQueryable<OrderStatisticDto> GetOrdersStatisticDto(
        IQueryable<IGrouping<YearKey, Order>> groupedResult)
    {
        return groupedResult
            .Select(g => new OrderStatisticDto()
            {
                Year = g.Key.Year,
                Month = (g.Key as MonthKey) == null ? null : (g.Key as MonthKey)!.Month,
                Day = (g.Key as DayKey) == null ? null : (g.Key as DayKey)!.Day,
                NumberOfOrders = g.Select(o => o.Id).DefaultIfEmpty().Count(),
                TotalDiscount = g.SelectMany(o => o.OrderRecords.Select(or => or.Discount))
                    .DefaultIfEmpty().Sum(),
                TotalCost = g.SelectMany(o => o.OrderRecords
                        .Select(or => or.Price * or.Quantity - or.Discount)
                    .DefaultIfEmpty()).Sum(),
            });
    }

    public async Task<IEnumerable<EmailLog>> GetEmailLogsAsync(string? recipientEmail, string? subject,
        DateOnly? startDateTime, DateOnly? endDateTime, CancellationToken cancellationToken)
    {
        IQueryable<EmailLog> emails = context.EmailLogs;
        if(recipientEmail != null)
        {
            emails = emails.Where(e => e.RecipientEmail.ToLower().Contains(recipientEmail.ToLower()));
        }
        if(subject != null)
        {
            emails = emails.Where(e => e.Subject.ToLower().Contains(subject.ToLower()));
        }
        if(startDateTime != null)
        {
            emails = emails.Where(e => e.SendAt >= startDateTime.Value.ToDateTime(new TimeOnly(0, 0, 0)));
        }
        if (endDateTime != null)
        {
            emails = emails.Where(e => e.SendAt <= endDateTime.Value.ToDateTime(new TimeOnly(23, 59, 59)));
        }
        return await emails.ToListAsync(cancellationToken);
    }

    public async Task AddToProductNumberViewsAsync(int productId)
    {
        ProductStatistics productStatistic = await GetProductStatisticsAsync(productId);
        productStatistic.NumberViews++;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding number of views to the statistic of the product with id {productId}", productId);
        }
    }

    public async Task AddToProductNumberPurchasesAsync(OrderDto order)
    {
        try
        {
            foreach (var record in order.OrderRecords)
            {
                var product = await context.Products
                    .Include(p => p.ProductInstances)
                    .FirstOrDefaultAsync(p => p.ProductInstances.Any(pi => pi.Id == record.ProductInstanceId));
                if(product == null)
                {
                    return;
                }
                var productStatistic = await GetProductStatisticsAsync(product.Id);
                var productInstanceStatistics = GetProductInstanceStatistics(productStatistic, record.ProductInstanceId);
                
                productInstanceStatistics.NumberOfPurchases += record.Quantity;
                productInstanceStatistics.TotalRevenue += record.TotalCost;
            }
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding adding statistics for the order {@order}", order);
        }
    }

    public async Task AddToProductNumberFeedbacksAsync(int productId)
    {
        try
        {
            ProductStatistics productStatistic = await GetProductStatisticsAsync(productId);
            productStatistic.NumberFeedbacks++;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding number of feedbacks to the statistic of the product with id {productId}", productId);
        }
    }

    public async Task AddToProductNumberWishlistAdditionsAsync(int productId)
    {
        try
        {
            ProductStatistics productStatistic = await GetProductStatisticsAsync(productId);
            productStatistic.NumberWishlistAdditions++;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding number of wish list addition to the statistic of the product with id {productId}", productId);
        }
    }

    private async Task<ProductStatistics> GetProductStatisticsAsync(int productId)
    {
        var date = DateTimeOffset.UtcNow;
        var productStatistic = await context.ProductStatistics
            .Include(p => p.ProductInstanceStatistics)
            .FirstOrDefaultAsync(ps => ps.ProductId == productId
                && ps.Year == date.Year && ps.Month == date.Month && ps.Day == date.Day);
        if (productStatistic == null)
        {
            productStatistic = new ProductStatistics()
            {
                ProductId = productId,
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                ProductInstanceStatistics = []
            };
            await context.ProductStatistics.AddAsync(productStatistic);
        }
        return productStatistic;
    }
    private static ProductInstanceStatistics GetProductInstanceStatistics(
        ProductStatistics productStatistics, int productInstanceId)
    {
        var productInstanceStatistics = productStatistics.ProductInstanceStatistics
                    .Find(pis => pis.ProductInstanceId == productInstanceId);
        if (productInstanceStatistics == null)
        {
            productInstanceStatistics = new ProductInstanceStatistics()
            {
                ProductInstanceId = productInstanceId,
            };
            productStatistics.ProductInstanceStatistics.Add(productInstanceStatistics);
        }
        return productInstanceStatistics;
    }

    private class YearKey
    {
        public int? Year { get; set; }
    }
    private class MonthKey : YearKey
    {
        public int? Month { get; set; }
    }
    private sealed class DayKey : MonthKey
    {
        public int? Day { get; set; }
    }
    private class AllTimeProductKey
    {
        public int ProductId { get; set; }
    }
    private class YearProductKey : AllTimeProductKey
    {
        public int Year { get; set; }
    }
    private class MonthProductKey : YearProductKey
    {
        public int Month { get; set; }
    }
    private sealed class DayProductKey : MonthProductKey
    {
        public int Day { get; set; }
    }
}
