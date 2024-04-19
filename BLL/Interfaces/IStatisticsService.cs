using HM.BLL.Models;
using HM.BLL.Models.Statistics;
using HM.DAL.Entities;

namespace HM.BLL.Interfaces;

public interface IStatisticsService
{
    Task<IEnumerable<ProductStatisticDto>> GetProductStatisticsAsync(int? productId, int? categoryId,
        int? categoryGroupId, DateOnly? fromDate, DateOnly? toDate, bool yearly, bool monthly, bool daily,
        bool sortByDateFirst, bool sortByDateAsc, bool sortByProductAsc, CancellationToken cancellationToken);
    Task<IEnumerable<CategoryStatisticDto>> GetCategoryStatisticsAsync(
        int? categoryGroupId, int? categoryId, DateOnly? fromDate, DateOnly? toDate,
        bool yearly, bool monthly, bool daily, CancellationToken cancellationToken);

    Task<IEnumerable<OrderStatisticDto>> GetOrderStatisticsAsync(
        DateOnly? fromDate, DateOnly? toDate,
        bool yearly, bool monthly, bool daily, CancellationToken cancellationToken);

    Task<IEnumerable<EmailLog>> GetEmailLogsAsync(string? recipientEmail, string? subject,
        DateOnly? startDateTime, DateOnly? endDateTime, CancellationToken cancellationToken);

    Task AddToProductNumberViews(int productId);
    Task AddToProductNumberPurchases(OrderDto order);
    Task AddToProductNumberFeedbacks(int productId);
    Task AddToProductNumberWishlistAdditions(int productId);

}
