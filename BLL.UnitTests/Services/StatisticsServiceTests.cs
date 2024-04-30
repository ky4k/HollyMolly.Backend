using HM.BLL.Models.Orders;
using HM.BLL.Models.Statistics;
using HM.BLL.Services;
using HM.BLL.UnitTests.TestHelpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;

namespace HM.BLL.UnitTests.Services;

public class StatisticsServiceTests
{
    private readonly HmDbContext _context;
    private readonly StatisticsService _statisticsService;
    private readonly ILogger<StatisticsService> _logger;
    public StatisticsServiceTests()
    {
        _logger = Substitute.For<ILogger<StatisticsService>>();
        _context = ServiceHelper.GetTestDbContext();
        _statisticsService = new StatisticsService(_context, _logger);
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnGeneralizedStatisticsForEachProductForAllTime_WhenNoParametersSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, false, false, false, false, CancellationToken.None);
        ProductStatisticDto? product1Statistic = statisticDtos.FirstOrDefault(s => s.ProductId == 1);

        Assert.Equal(4, statisticDtos.Count());
        Assert.NotNull(product1Statistic);
        Assert.Multiple(
            () => Assert.Equal(200, product1Statistic.NumberViews),
            () => Assert.Equal(20, product1Statistic.NumberPurchases),
            () => Assert.Equal(0.1m, product1Statistic.ConversionRate),
            () => Assert.Equal(0.5m, product1Statistic.Rating),
            () => Assert.Equal(100, product1Statistic.NumberReviews),
            () => Assert.Equal(60, product1Statistic.NumberWishListAddition),
            () => Assert.Equal(600, product1Statistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticsForTheSpecifiedProduct()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            1, null, null, null, null, false, false, false, false, false, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.First();

        Assert.Single(statisticDtos);
        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(200, productStatistic.NumberViews),
            () => Assert.Equal(20, productStatistic.NumberPurchases),
            () => Assert.Equal(0.1m, productStatistic.ConversionRate),
            () => Assert.Equal(0.5m, productStatistic.Rating),
            () => Assert.Equal(100, productStatistic.NumberReviews),
            () => Assert.Equal(60, productStatistic.NumberWishListAddition),
            () => Assert.Equal(600, productStatistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticsForTheSpecifiedCategory()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, 1, null, null, null, false, false, false, false, false, false, CancellationToken.None);

        Assert.Equal(2, statisticDtos.Count());
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticsForTheSpecifiedCategoryGroup()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, 1, null, null, false, false, false, false, false, false, CancellationToken.None);

        Assert.Equal(3, statisticDtos.Count());
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldFilterStatisticForTimePeriodAfterSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, new DateOnly(2024, 4, 1), null, false, false, false, false, false, false, CancellationToken.None);
        ProductStatisticDto? product1Statistic = statisticDtos.FirstOrDefault(s => s.ProductId == 1);

        Assert.Equal(4, statisticDtos.Count());
        Assert.NotNull(product1Statistic);
        Assert.Multiple(
            () => Assert.Equal(100, product1Statistic.NumberViews),
            () => Assert.Equal(12, product1Statistic.NumberPurchases),
            () => Assert.Equal(0.12m, product1Statistic.ConversionRate),
            () => Assert.Equal(0.5m, product1Statistic.Rating),
            () => Assert.Equal(50, product1Statistic.NumberReviews),
            () => Assert.Equal(30, product1Statistic.NumberWishListAddition),
            () => Assert.Equal(340, product1Statistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldFilterStatisticForTimePeriodBeforeSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, new DateOnly(2024, 3, 31), false, false, false, false, false, false, CancellationToken.None);
        ProductStatisticDto? product1Statistic = statisticDtos.FirstOrDefault(s => s.ProductId == 1);

        Assert.Equal(4, statisticDtos.Count());
        Assert.NotNull(product1Statistic);
        Assert.Multiple(
            () => Assert.Equal(100, product1Statistic.NumberViews),
            () => Assert.Equal(8, product1Statistic.NumberPurchases),
            () => Assert.Equal(0.08m, product1Statistic.ConversionRate),
            () => Assert.Equal(0.5m, product1Statistic.Rating),
            () => Assert.Equal(50, product1Statistic.NumberReviews),
            () => Assert.Equal(30, product1Statistic.NumberWishListAddition),
            () => Assert.Equal(260, product1Statistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldFilterStatisticForTimePeriodBetweenSpecifiedDates()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 1), false, false, false, false, false, false, CancellationToken.None);
        ProductStatisticDto? product1Statistic = statisticDtos.FirstOrDefault(s => s.ProductId == 1);

        Assert.Equal(4, statisticDtos.Count());
        Assert.NotNull(product1Statistic);
        Assert.Multiple(
            () => Assert.Equal(50, product1Statistic.NumberViews),
            () => Assert.Equal(4, product1Statistic.NumberPurchases),
            () => Assert.Equal(0.08m, product1Statistic.ConversionRate),
            () => Assert.Equal(0.5m, product1Statistic.Rating),
            () => Assert.Equal(25, product1Statistic.NumberReviews),
            () => Assert.Equal(15, product1Statistic.NumberWishListAddition),
            () => Assert.Equal(120, product1Statistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticForEachProductForEachYear_WhenYearlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, true, false, false, false, false, false, CancellationToken.None);
        List<ProductStatisticDto> productStatistics = statisticDtos.Where(p => p.ProductId == 1).ToList();

        Assert.Equal(8, statisticDtos.Count());
        Assert.Equal(2, productStatistics.Count);
        Assert.Multiple(
            () => Assert.Equal(150, productStatistics[0].NumberViews),
            () => Assert.Equal(16, productStatistics[0].NumberPurchases),
            () => Assert.Equal(0.5m, productStatistics[0].Rating),
            () => Assert.Equal(75, productStatistics[0].NumberReviews),
            () => Assert.Equal(45, productStatistics[0].NumberWishListAddition),
            () => Assert.Equal(460, productStatistics[0].TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticForEachProductForEachMonth_WhenMonthlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, true, true, false, false, false, false, CancellationToken.None);
        List<ProductStatisticDto> productStatistics = statisticDtos.Where(p => p.ProductId == 1).ToList();

        Assert.Equal(12, statisticDtos.Count());
        Assert.Equal(3, productStatistics.Count);
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnStatisticForEachProductForEachDay_WhenDailyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, true, true, true, false, false, false, CancellationToken.None);
        List<ProductStatisticDto> productStatistics = statisticDtos.Where(p => p.ProductId == 1).ToList();

        Assert.Equal(20, statisticDtos.Count());
        Assert.Equal(5, productStatistics.Count);
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnCorrectDailyStatistics_WhenDailyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            1, null, null, null, null, true, true, true, false, false, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos
            .FirstOrDefault(s => s.Year == 2024 && s.Month == 4 && s.Day == 3);

        Assert.Equal(5, statisticDtos.Count());
        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(20, productStatistic.NumberViews),
            () => Assert.Equal(4, productStatistic.NumberPurchases),
            () => Assert.Equal(0.2m, productStatistic.ConversionRate),
            () => Assert.Equal(0.5m, productStatistic.Rating),
            () => Assert.Equal(10, productStatistic.NumberReviews),
            () => Assert.Equal(2, productStatistic.NumberWishListAddition),
            () => Assert.Equal(100, productStatistic.TotalRevenue)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByDateAscending_WhenSortByDateFirstIsTrueAndSortByDateAscIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, true, true, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(2023, productStatistic.Year),
            () => Assert.Equal(11, productStatistic.Month),
            () => Assert.Equal(20, productStatistic.Day),
            () => Assert.Equal(2, productStatistic.ProductId)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByDateDescending_WhenSortByDateFirstIsTrueAndSortByDateAscIsFalse()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, true, false, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(2024, productStatistic.Year),
            () => Assert.Equal(4, productStatistic.Month),
            () => Assert.Equal(23, productStatistic.Day),
            () => Assert.Equal(3, productStatistic.ProductId)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByProductIdAscending_WhenSortByDateFirstIsFalseAndSortByProductAscIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, false, false, true, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(1, productStatistic.ProductId),
            () => Assert.Equal(2024, productStatistic.Year),
            () => Assert.Equal(4, productStatistic.Month),
            () => Assert.Equal(3, productStatistic.Day)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByProductIdDescending_WhenSortByDateFirstIsFalseAndSortByProductAscIsFalse()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, false, false, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(5, productStatistic.ProductId),
            () => Assert.Equal(2024, productStatistic.Year),
            () => Assert.Equal(4, productStatistic.Month),
            () => Assert.Equal(3, productStatistic.Day)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByDateDescendingThenByProductAscending_WhenSortByDateFirstIsTrueAndSortByDateAscIsFalseAndSortByProductAscIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, true, false, true, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(2024, productStatistic.Year),
            () => Assert.Equal(4, productStatistic.Month),
            () => Assert.Equal(23, productStatistic.Day),
            () => Assert.Equal(3, productStatistic.ProductId)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldSortResultByProductDescendingThenByDateAscending_WhenSortByDateFirstIsFalseAndSortByDateAscIsTrueAndSortByProductAscIsFalse()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            null, null, null, null, null, false, false, true, false, true, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault();

        Assert.NotNull(productStatistic);
        Assert.Multiple(
            () => Assert.Equal(5, productStatistic.ProductId),
            () => Assert.Equal(2023, productStatistic.Year),
            () => Assert.Equal(12, productStatistic.Month),
            () => Assert.Equal(20, productStatistic.Day)
        );
    }
    [Fact]
    public async Task GetProductStatisticsAsync_ShouldReturnCorrectConversionRate_WhenProductHasNoViews()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductStatisticDto> statisticDtos = await _statisticsService.GetProductStatisticsAsync(
            5, null, null, null, null, false, false, true, false, true, false, CancellationToken.None);
        ProductStatisticDto? productStatistic = statisticDtos.FirstOrDefault(s => s.NumberViews == 0);

        Assert.NotNull(productStatistic);
        Assert.Equal(0, productStatistic.ConversionRate);
    }

    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldReturnGeneralizedStatisticForTheSpecifiedCategoryGroup()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, null, null, false, false, false, CancellationToken.None);
        CategoryStatisticDto? categoryStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(categoryStatistic);
        Assert.Multiple(
            () => Assert.Equal(610, categoryStatistic.NumberProductViews),
            () => Assert.Equal(20, categoryStatistic.NumberPurchases),
            () => Assert.Equal(600, categoryStatistic.TotalRevenue),
            () => Assert.Equal(320, categoryStatistic.NumberReviews),
            () => Assert.Equal(180, categoryStatistic.WishListAddition)
        );
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldReturnGeneralizedStatisticForTheSpecifiedCategory()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            null, 1, null, null, false, false, false, CancellationToken.None);
        CategoryStatisticDto? categoryStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(categoryStatistic);
        Assert.Multiple(
            () => Assert.Equal(405, categoryStatistic.NumberProductViews),
            () => Assert.Equal(20, categoryStatistic.NumberPurchases),
            () => Assert.Equal(600, categoryStatistic.TotalRevenue),
            () => Assert.Equal(210, categoryStatistic.NumberReviews),
            () => Assert.Equal(120, categoryStatistic.WishListAddition)
        );
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldReturnEmptyCollection_WhenNeitherCategoryGroupNorCategoryWereSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            null, null, null, null, false, false, false, CancellationToken.None);

        Assert.Empty(statisticDtos);
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldReturnEmptyCollection_WhenCategoryGroupWasSpecifiedButNotExist()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            999, null, null, null, false, false, false, CancellationToken.None);

        Assert.Empty(statisticDtos);
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldReturnEmptyCollection_WhenCategoryWasSpecifiedButNotExist()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            null, 999, null, null, false, false, false, CancellationToken.None);

        Assert.Empty(statisticDtos);
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldFilterStatisticForTimePeriodAfterSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, new DateOnly(2024, 4, 1), null, false, false, false, CancellationToken.None);
        CategoryStatisticDto? categoryStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(categoryStatistic);
        Assert.Multiple(
            () => Assert.Equal(310, categoryStatistic.NumberProductViews),
            () => Assert.Equal(12, categoryStatistic.NumberPurchases),
            () => Assert.Equal(340, categoryStatistic.TotalRevenue),
            () => Assert.Equal(170, categoryStatistic.NumberReviews),
            () => Assert.Equal(90, categoryStatistic.WishListAddition)
        );
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldFilterStatisticForTimePeriodBeforeSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, null, new DateOnly(2024, 3, 31), false, false, false, CancellationToken.None);
        CategoryStatisticDto? categoryStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(categoryStatistic);
        Assert.Multiple(
            () => Assert.Equal(300, categoryStatistic.NumberProductViews),
            () => Assert.Equal(8, categoryStatistic.NumberPurchases),
            () => Assert.Equal(260, categoryStatistic.TotalRevenue),
            () => Assert.Equal(150, categoryStatistic.NumberReviews),
            () => Assert.Equal(90, categoryStatistic.WishListAddition)
        );
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldFilterStatisticForTimePeriodBetweenSpecifiedDates()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 1), false, false, false, CancellationToken.None);
        CategoryStatisticDto? categoryStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(categoryStatistic);
        Assert.Multiple(
            () => Assert.Equal(150, categoryStatistic.NumberProductViews),
            () => Assert.Equal(4, categoryStatistic.NumberPurchases),
            () => Assert.Equal(120, categoryStatistic.TotalRevenue),
            () => Assert.Equal(75, categoryStatistic.NumberReviews),
            () => Assert.Equal(45, categoryStatistic.WishListAddition)
        );
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldShouldReturnStatisticForEachYear_WhenYearlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, null, null, true, false, false, CancellationToken.None);

        Assert.Equal(2, statisticDtos.Count());
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldShouldReturnStatisticForEachMonth_WhenMonthlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, null, null, true, true, false, CancellationToken.None);

        Assert.Equal(4, statisticDtos.Count());
    }
    [Fact]
    public async Task GetCategoryStatisticsAsync_ShouldShouldReturnStatisticForEachDay_WhenDailyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryStatisticDto> statisticDtos = await _statisticsService.GetCategoryStatisticsAsync(
            1, null, null, null, true, true, true, CancellationToken.None);

        Assert.Equal(7, statisticDtos.Count());
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldReturnAllTimeGeneralizedStatistic_WhenNoParametersWereSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, false, false, false, false, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(5, orderStatistic.NumberOfOrders),
            () => Assert.Equal(650, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(75, orderStatistic.TotalDiscount),
            () => Assert.Equal(575, orderStatistic.TotalCost),
            () => Assert.Equal(115, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldFilterStatisticsAfterSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            new DateOnly(2024, 4, 1), null, false, false, false, false, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(3, orderStatistic.NumberOfOrders),
            () => Assert.Equal(390, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(45, orderStatistic.TotalDiscount),
            () => Assert.Equal(345, orderStatistic.TotalCost),
            () => Assert.Equal(115, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldFilterStatisticsBeforeSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, new DateOnly(2024, 3, 31), false, false, false, false, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(2, orderStatistic.NumberOfOrders),
            () => Assert.Equal(260, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(30, orderStatistic.TotalDiscount),
            () => Assert.Equal(230, orderStatistic.TotalCost),
            () => Assert.Equal(115, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldFilterStatisticsBetweenSpecifiedDates()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 1), false, false, false, false, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(1, orderStatistic.NumberOfOrders),
            () => Assert.Equal(130, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(15, orderStatistic.TotalDiscount),
            () => Assert.Equal(115, orderStatistic.TotalCost),
            () => Assert.Equal(115, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldReturnStatisticsForEveryYear_WhenYearlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, true, false, false, false, CancellationToken.None);

        Assert.Equal(2, statisticDtos.Count());
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldReturnStatisticsForEveryMonth_WhenMonthlyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, true, true, false, false, CancellationToken.None);

        Assert.Equal(3, statisticDtos.Count());
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldReturnStatisticsForEveryDay_WhenDailyIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, true, true, true, false, CancellationToken.None);

        Assert.Equal(5, statisticDtos.Count());
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldIncludeUnpaidOrdersIntoStatistics_WhenIncludeUnpaidIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, false, false, true, true, CancellationToken.None);

        Assert.Equal(6, statisticDtos.Count());
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldAddUnpaidOrdersToTheStatistics_WhenIncludeUnpaidIsTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, null, false, false, false, true, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(6, orderStatistic.NumberOfOrders),
            () => Assert.Equal(780, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(90, orderStatistic.TotalDiscount),
            () => Assert.Equal(690, orderStatistic.TotalCost),
            () => Assert.Equal(115, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetOrderStatisticsAsync_ShouldReturnStatistics_WhenThereAreNoOrders()
    {
        await SeedDbContextAsync();

        IEnumerable<OrderStatisticDto> statisticDtos = await _statisticsService.GetOrderStatisticsAsync(
            null, new DateOnly(2022, 1, 1), false, false, false, false, CancellationToken.None);
        OrderStatisticDto? orderStatistic = statisticDtos.FirstOrDefault();

        Assert.Single(statisticDtos);
        Assert.NotNull(orderStatistic);
        Assert.Multiple(
            () => Assert.Equal(0, orderStatistic.NumberOfOrders),
            () => Assert.Equal(0, orderStatistic.TotalCostBeforeDiscount),
            () => Assert.Equal(0, orderStatistic.TotalDiscount),
            () => Assert.Equal(0, orderStatistic.TotalCost),
            () => Assert.Equal(0, orderStatistic.AverageOrderCost)
        );
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldReturnAllEmailLogs_WhenNoFilterWasSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            null, null, null, null, CancellationToken.None);

        Assert.Equal(3, emailLogs.Count());
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldFilterEmailsByRecipient()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            "test", null, null, null, CancellationToken.None);

        Assert.Equal(2, emailLogs.Count());
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldFilterEmailsBySubject()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            null, "suBj", null, null, CancellationToken.None);

        Assert.Equal(2, emailLogs.Count());
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldFilterEmailLogsAfterSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            null, null, new DateOnly(2024, 2, 1), null, CancellationToken.None);

        Assert.Equal(2, emailLogs.Count());
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldFilterEmailLogsBeforeSpecifiedDate()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            null, null, null, new DateOnly(2024, 3, 1), CancellationToken.None);

        Assert.Equal(2, emailLogs.Count());
    }
    [Fact]
    public async Task GetEmailLogsAsync_ShouldFilterEmailLogsBetweenSpecifiedDates()
    {
        await SeedDbContextAsync();

        IEnumerable<EmailLog> emailLogs = await _statisticsService.GetEmailLogsAsync(
            null, null, new DateOnly(2024, 2, 1), new DateOnly(2024, 3, 1), CancellationToken.None);

        Assert.Single(emailLogs);
    }
    [Fact]
    public async Task AddToProductNumberViewsAsync_ShouldCreateNewStatisticAndAddNumberView_WhenStatisticDoesNotExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberViewsAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(1, productStatistics.NumberViews);
    }
    [Fact]
    public async Task AddToProductNumberViewsAsync_ShouldAddNumberView_WhenTheStatisticExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberViewsAsync(1);
        await _statisticsService.AddToProductNumberViewsAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(2, productStatistics.NumberViews);
    }

    [Fact]
    public async Task AddToProductNumberViewsAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new StatisticsService(dbContextMock, _logger);

        Exception? exception = await Record.ExceptionAsync(
            async () => await service.AddToProductNumberViewsAsync(1));

        Assert.Null(exception);
    }

    [Fact]
    public async Task AddToProductNumberPurchasesAsync_ShouldAddToEveryProductInstanceInOrder()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;
        OrderDto orderDto = new()
        {
            OrderDate = today,
            OrderRecords =
            [
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    Discount = 10
                },
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Quantity = 2,
                    Price = 40,
                    Discount = 20
                }
            ]
        };

        await _statisticsService.AddToProductNumberPurchasesAsync(orderDto);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(2, productStatistics.ProductInstanceStatistics.Count);
        Assert.Equal(1, productStatistics.ProductInstanceStatistics[0].NumberOfPurchases);
        Assert.Equal(90, productStatistics.ProductInstanceStatistics[0].TotalRevenue);
        Assert.Equal(2, productStatistics.ProductInstanceStatistics[1].NumberOfPurchases);
        Assert.Equal(60, productStatistics.ProductInstanceStatistics[1].TotalRevenue);
    }
    [Fact]
    public async Task AddToProductNumberPurchasesAsync_ShouldCreateStatistic_WhenStatisticDoesNotExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;
        OrderDto orderDto = new()
        {
            OrderDate = today,
            OrderRecords =
            [
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    Discount = 10
                },
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Quantity = 2,
                    Price = 40,
                    Discount = 20
                }
            ]
        };

        await _statisticsService.AddToProductNumberPurchasesAsync(orderDto);
        await _statisticsService.AddToProductNumberPurchasesAsync(orderDto);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(2, productStatistics.ProductInstanceStatistics.Count);
        Assert.Equal(2, productStatistics.ProductInstanceStatistics[0].NumberOfPurchases);
        Assert.Equal(180, productStatistics.ProductInstanceStatistics[0].TotalRevenue);
        Assert.Equal(4, productStatistics.ProductInstanceStatistics[1].NumberOfPurchases);
        Assert.Equal(120, productStatistics.ProductInstanceStatistics[1].TotalRevenue);
    }
    [Fact]
    public async Task AddToProductNumberPurchasesAsync_ShouldNotModifyStatistics_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;
        OrderDto orderDto = new()
        {
            OrderDate = today,
            OrderRecords =
            [
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 999,
                    Quantity = 1,
                    Price = 100,
                    Discount = 10
                }
            ]
        };

        await _statisticsService.AddToProductNumberPurchasesAsync(orderDto);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.Null(productStatistics);
    }
    [Fact]
    public async Task AddToProductNumberPurchasesAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new StatisticsService(dbContextMock, _logger);
        var today = DateTimeOffset.UtcNow;
        OrderDto orderDto = new()
        {
            OrderDate = today,
            OrderRecords =
            [
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    Discount = 10
                },
                new()
                {
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Quantity = 2,
                    Price = 40,
                    Discount = 20
                }
            ]
        };

        Exception? exception = await Record.ExceptionAsync(
            async () => await service.AddToProductNumberPurchasesAsync(orderDto));

        Assert.Null(exception);
    }
    [Fact]
    public async Task AddToProductNumberFeedbacksAsync_ShouldCreateStatistic_WhenStatisticDoesNotAlreadyExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberFeedbacksAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(1, productStatistics.NumberFeedbacks);
    }
    [Fact]
    public async Task AddToProductNumberFeedbacksAsync_ShouldAddNumberFeedback_WhenTheStatisticExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberFeedbacksAsync(1);
        await _statisticsService.AddToProductNumberFeedbacksAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(2, productStatistics.NumberFeedbacks);
    }
    [Fact]
    public async Task AddToProductNumberFeedbacksAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new StatisticsService(dbContextMock, _logger);

        Exception? exception = await Record.ExceptionAsync(
            async () => await service.AddToProductNumberFeedbacksAsync(1));

        Assert.Null(exception);
    }
    [Fact]
    public async Task AddToProductNumberWishlistAdditionsAsync_ShouldCreateStatistic_WhenStatisticDoesNotAlreadyExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberWishlistAdditionsAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(1, productStatistics.NumberWishlistAdditions);
    }
    [Fact]
    public async Task AddToProductNumberWishlistAdditionsAsync_ShouldAddNumberWishlistAdditions_WhenTheStatisticExist()
    {
        await SeedDbContextAsync();
        var today = DateTimeOffset.UtcNow;

        await _statisticsService.AddToProductNumberWishlistAdditionsAsync(1);
        await _statisticsService.AddToProductNumberWishlistAdditionsAsync(1);
        ProductStatistics? productStatistics = await _context.ProductStatistics
            .FirstOrDefaultAsync(s => s.ProductId == 1
                && s.Year == today.Year && s.Month == today.Month && s.Day == today.Day);

        Assert.NotNull(productStatistics);
        Assert.Equal(2, productStatistics.NumberWishlistAdditions);
    }
    [Fact]
    public async Task AddToProductNumberWishlistAdditionsAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new StatisticsService(dbContextMock, _logger);

        Exception? exception = await Record.ExceptionAsync(
            async () => await service.AddToProductNumberWishlistAdditionsAsync(1));

        Assert.Null(exception);
    }
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(2024, null, null)]
    [InlineData(null, 4, null)]
    [InlineData(null, null, 1)]
    [InlineData(2024, 4, 1)]
    [InlineData(null, 4, 1)]
    public void GroupKeyEquals_ShouldReturnTrue_WhenKeysAreEqual(int? year, int? month, int? day)
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        dynamic other = Activator.CreateInstance(groupKeyType)!;
        PropertyInfo yearPropertyInfo = groupKeyType.GetProperty("Year")!;
        yearPropertyInfo.SetValue(current, year);
        yearPropertyInfo.SetValue(other, year);
        PropertyInfo monthPropertyInfo = groupKeyType.GetProperty("Month")!;
        monthPropertyInfo.SetValue(current, month);
        monthPropertyInfo.SetValue(other, month);
        PropertyInfo dayPropertyInfo = groupKeyType.GetProperty("Day")!;
        dayPropertyInfo.SetValue(current, day);
        dayPropertyInfo.SetValue(other, day);

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.True(equal);
        Assert.Equal(currentHashCode, otherHashCode);
    }
    [Theory]
    [InlineData(2024, 4, 1, 2024, 4, 2)]
    [InlineData(2024, 4, 1, 2024, 4, null)]
    [InlineData(2024, 4, null, 2024, 4, 1)]
    [InlineData(2024, 4, 1, 2024, 3, 1)]
    [InlineData(2024, 4, 1, 2023, 4, 1)]
    [InlineData(2023, 3, 2, 2024, 4, 1)]
    [InlineData(null, null, 1, null, null, 2)]
    [InlineData(null, null, 1, null, null, null)]
    [InlineData(2024, null, null, null, null, null)]
    public void GroupKeyEquals_ShouldReturnFalse_WhenKeysAreNotEqual(int? year1, int? month1, int? day1, int? year2, int? month2, int? day2)
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        dynamic other = Activator.CreateInstance(groupKeyType)!;
        PropertyInfo yearPropertyInfo = groupKeyType.GetProperty("Year")!;
        yearPropertyInfo.SetValue(current, year1);
        yearPropertyInfo.SetValue(other, year2);
        PropertyInfo monthPropertyInfo = groupKeyType.GetProperty("Month")!;
        monthPropertyInfo.SetValue(current, month1);
        monthPropertyInfo.SetValue(other, month2);
        PropertyInfo dayPropertyInfo = groupKeyType.GetProperty("Day")!;
        dayPropertyInfo.SetValue(current, day1);
        dayPropertyInfo.SetValue(other, day2);

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.False(equal);
        Assert.NotEqual(currentHashCode, otherHashCode);
    }
    [Fact]
    public void GroupKeyEquals_ShouldReturnFalse_WhenComparedToOherType()
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        object other = new();

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.False(equal);
        Assert.NotEqual(currentHashCode, otherHashCode);
    }
    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(null, 2024, null, null)]
    [InlineData(null, null, 4, null)]
    [InlineData(null, null, null, 1)]
    [InlineData(null, 2024, 4, 1)]
    [InlineData(10, null, 4, 1)]
    [InlineData(10, 2024, null, null)]
    [InlineData(null, null, 4, 1)]
    [InlineData(10, null, null, 1)]
    public void GroupProductKeyEquals_ShouldReturnTrue_WhenKeysAreEqual(
        int? productId, int? year, int? month, int? day)
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupProductKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        dynamic other = Activator.CreateInstance(groupKeyType)!;
        PropertyInfo productIdPropertyInfo = groupKeyType.GetProperty("ProductId")!;
        productIdPropertyInfo.SetValue(current, productId);
        productIdPropertyInfo.SetValue(other, productId);
        PropertyInfo yearPropertyInfo = groupKeyType.GetProperty("Year")!;
        yearPropertyInfo.SetValue(current, year);
        yearPropertyInfo.SetValue(other, year);
        PropertyInfo monthPropertyInfo = groupKeyType.GetProperty("Month")!;
        monthPropertyInfo.SetValue(current, month);
        monthPropertyInfo.SetValue(other, month);
        PropertyInfo dayPropertyInfo = groupKeyType.GetProperty("Day")!;
        dayPropertyInfo.SetValue(current, day);
        dayPropertyInfo.SetValue(other, day);

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.True(equal);
        Assert.Equal(currentHashCode, otherHashCode);
    }
    [Theory]
    [InlineData(10, 2024, 4, 1, 10, 2024, 4, 2)]
    [InlineData(10, 2024, 4, 1, 10, 2024, 4, null)]
    [InlineData(10, 2024, 4, 1, null, 2024, 4, 1)]
    [InlineData(10, 2024, 4, null, 10, 2024, 4, 1)]
    [InlineData(10, 2024, 4, 1, 11, 2024, 4, 1)]
    [InlineData(10, 2024, 4, 1, 10, 2024, 3, 1)]
    [InlineData(10, 2024, 4, 1, 10, 2023, 4, 1)]
    [InlineData(10, 2023, 3, 2, 10, 2024, 4, 1)]
    [InlineData(null, null, null, 1, null, null, null, 2)]
    [InlineData(10, null, null, null, 11, null, null, null)]
    [InlineData(null, null, null, null, 11, null, null, null)]
    [InlineData(null, null, null, 1, null, null, null, null)]
    [InlineData(null, 2024, null, null, null, null, null, null)]
    public void GroupProductKeysEqual_ShouldReturnFalse_WhenKeysAreNotEqual(
        int? productId1, int? year1, int? month1, int? day1, int? productId2, int? year2, int? month2, int? day2)
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupProductKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        dynamic other = Activator.CreateInstance(groupKeyType)!;
        PropertyInfo productIdPropertyInfo = groupKeyType.GetProperty("ProductId")!;
        productIdPropertyInfo.SetValue(current, productId1);
        productIdPropertyInfo.SetValue(other, productId2);
        PropertyInfo yearPropertyInfo = groupKeyType.GetProperty("Year")!;
        yearPropertyInfo.SetValue(current, year1);
        yearPropertyInfo.SetValue(other, year2);
        PropertyInfo monthPropertyInfo = groupKeyType.GetProperty("Month")!;
        monthPropertyInfo.SetValue(current, month1);
        monthPropertyInfo.SetValue(other, month2);
        PropertyInfo dayPropertyInfo = groupKeyType.GetProperty("Day")!;
        dayPropertyInfo.SetValue(current, day1);
        dayPropertyInfo.SetValue(other, day2);

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.False(equal);
        Assert.NotEqual(currentHashCode, otherHashCode);
    }
    [Fact]
    public void GroupProductKeyEquals_ShouldReturnFalse_WhenComparedToOherType()
    {
        Type statisticServiceType = typeof(StatisticsService);
        Type? groupKeyType = statisticServiceType.GetNestedType("GroupProductKey", BindingFlags.NonPublic)!;
        dynamic current = Activator.CreateInstance(groupKeyType)!;
        object other = new();

        bool equal = current.Equals(other);
        int currentHashCode = current.GetHashCode();
        int otherHashCode = other.GetHashCode();

        Assert.False(equal);
        Assert.NotEqual(currentHashCode, otherHashCode);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }
    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.CategoryGroups.AddRangeAsync(CategoryGroups);
        await context.Categories.AddRangeAsync(Categories);
        await context.Products.AddRangeAsync(Products);
        await context.Orders.AddRangeAsync(Orders);
        await context.EmailLogs.AddRangeAsync(EmailLogs);
        await context.SaveChangesAsync();
    }

    private static List<CategoryGroup> CategoryGroups =>
    [
        new()
        {
            Id = 1,
            Name = "Category group 1"
        },
        new()
        {
            Id = 2,
            Name = "Category group 2"
        }
    ];
    private static List<Category> Categories =>
    [
        new()
        {
            Id = 1,
            CategoryGroupId = 1,
            Name = "Category 1"
        },
        new()
        {
            Id = 2,
            CategoryGroupId = 1,
            Name = "Category 2"
        },
        new()
        {
            Id = 3,
            CategoryGroupId = 2,
            Name = "Category 3"
        },
        new()
        {
            Id = 4,
            CategoryGroupId = 2,
            Name = "Category 4"
        }
    ];
    private static List<Product> Products =>
    [
        new()
        {
            Id = 1,
            CategoryId = 1,
            Name = "Product 1",
            Rating = 0.5m,
            TimesRated = 10,
            ProductStatistics =
            [
                new()
                {
                    Id = 1,
                    Year = 2023,
                    Month = 12,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 2,
                    Year = 2024,
                    Month = 3,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 3,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    ProductId = 1,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 4,
                    Year = 2024,
                    Month = 4,
                    Day = 2,
                    NumberViews = 30,
                    NumberFeedbacks = 15,
                    NumberWishlistAdditions = 13,
                },
                new()
                {
                    Id = 5,
                    Year = 2024,
                    Month = 4,
                    Day = 3,
                    NumberViews = 20,
                    NumberFeedbacks = 10,
                    NumberWishlistAdditions = 2,
                }
            ],
            ProductInstances =
            [
                new()
                {
                    Id = 1,
                    StockQuantity = 10,
                    Price = 55,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0,
                    ProductInstanceStatistics =
                    [
                        new()
                        {
                            Id = 1,
                            NumberOfPurchases = 2,
                            TotalRevenue = 70,
                            ProductStatisticsId = 1,
                        },
                        new()
                        {
                            Id = 2,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 2,
                        },
                        new()
                        {
                            Id = 3,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 3,
                        },
                        new()
                        {
                            Id = 4,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 4,
                        },
                        new()
                        {
                            Id = 5,
                            NumberOfPurchases = 2,
                            TotalRevenue = 50,
                            ProductStatisticsId = 5,
                        },
                    ]
                },
                new()
                {
                    Id = 2,
                    StockQuantity = 10,
                    Price = 55,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10,
                    ProductInstanceStatistics =
                    [
                        new()
                        {
                            Id = 6,
                            NumberOfPurchases = 2,
                            TotalRevenue = 70,
                            ProductStatisticsId = 1,
                        },
                        new()
                        {
                            Id = 7,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 2,
                        },
                        new()
                        {
                            Id = 8,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 3,
                        },
                        new()
                        {
                            Id = 9,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 4,
                        },
                        new()
                        {
                            Id = 10,
                            NumberOfPurchases = 2,
                            TotalRevenue = 50,
                            ProductStatisticsId = 5,
                        }
                    ]
                },
            ]
        },
        new()
        {
            Id = 2,
            CategoryId = 1,
            Name = "Product 2",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 3,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 4,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ],
            ProductStatistics =
            [
                new()
                {
                    Id = 6,
                    Year = 2023,
                    Month = 11,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 7,
                    Year = 2024,
                    Month = 3,
                    Day = 20,
                    ProductId = 2,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 8,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 9,
                    Year = 2024,
                    Month = 4,
                    Day = 2,
                    NumberViews = 33,
                    NumberFeedbacks = 23,
                    NumberWishlistAdditions = 13,
                },
                new()
                {
                    Id = 10,
                    Year = 2024,
                    Month = 4,
                    Day = 3,
                    NumberViews = 22,
                    NumberFeedbacks = 12,
                    NumberWishlistAdditions = 2,
                }
            ]
        },
        new()
        {
            Id = 3,
            CategoryId = 2,
            Name = "Product 3",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 5,
                    StockQuantity = 10,
                    Price = 60,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 6,
                    StockQuantity = 10,
                    Price = 70,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ],
            ProductStatistics =
            [
                new()
                {
                    Id = 11,
                    Year = 2023,
                    Month = 12,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 12,
                    Year = 2024,
                    Month = 3,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 13,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 14,
                    Year = 2024,
                    Month = 4,
                    Day = 2,
                    NumberViews = 33,
                    NumberFeedbacks = 23,
                    NumberWishlistAdditions = 13,
                },
                new()
                {
                    Id = 15,
                    Year = 2024,
                    Month = 4,
                    Day = 23,
                    NumberViews = 22,
                    NumberFeedbacks = 12,
                    NumberWishlistAdditions = 2,
                }
            ]
        },
        new()
        {
            Id = 4,
            CategoryId = 2,
            Name = "Product 4",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 7,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 8,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ]
        },
        new()
        {
            Id = 5,
            CategoryId = 3,
            Name = "Product 5",
            Rating = 0.5m,
            TimesRated = 10,
            ProductStatistics =
            [
                new()
                {
                    Id = 21,
                    Year = 2023,
                    Month = 12,
                    Day = 20,
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 22,
                    Year = 2024,
                    Month = 3,
                    Day = 20,
                    NumberViews = 100,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 23,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 0,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 24,
                    Year = 2024,
                    Month = 4,
                    Day = 2,
                    NumberViews = 33,
                    NumberFeedbacks = 23,
                    NumberWishlistAdditions = 13,
                },
                new()
                {
                    Id = 25,
                    Year = 2024,
                    Month = 4,
                    Day = 3,
                    NumberViews = 22,
                    NumberFeedbacks = 12,
                    NumberWishlistAdditions = 2,
                }
            ],
            ProductInstances =
            [
                new()
                {
                    Id = 9,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0,
                    ProductInstanceStatistics =
                    [
                        new()
                        {
                            Id = 11,
                            NumberOfPurchases = 2,
                            TotalRevenue = 70,
                            ProductStatisticsId = 21,
                        },
                        new()
                        {
                            Id = 12,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 22,
                        },
                        new()
                        {
                            Id = 13,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 23,
                        },
                        new()
                        {
                            Id = 14,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 24,
                        },
                        new()
                        {
                            Id = 15,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 25,
                        },
                    ]
                },
                new()
                {
                    Id = 10,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10,
                    ProductInstanceStatistics =
                    [
                        new()
                        {
                            Id = 16,
                            NumberOfPurchases = 2,
                            TotalRevenue = 70,
                            ProductStatisticsId = 21,
                        },
                        new()
                        {
                            Id = 17,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 22,
                        },
                        new()
                        {
                            Id = 18,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 23,
                        },
                        new()
                        {
                            Id = 19,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 24,
                        },
                        new()
                        {
                            Id = 20,
                            NumberOfPurchases = 2,
                            TotalRevenue = 60,
                            ProductStatisticsId = 25,
                        }
                    ]
                },
            ]
        },
        new()
        {
            Id = 6,
            CategoryId = 3,
            Name = "Product 6",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 11,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 12,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ]
        },
        new()
        {
            Id = 7,
            CategoryId = 4,
            Name = "Product 7",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 13,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 14,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ]
        },
        new()
        {
            Id = 8,
            CategoryId = 4,
            Name = "Product 8",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 15,
                    StockQuantity = 10,
                    Price = 75,
                    AbsoluteDiscount = 10,
                    PercentageDiscount = 0
                },
                new()
                {
                    Id = 16,
                    StockQuantity = 10,
                    Price = 80,
                    AbsoluteDiscount = 0,
                    PercentageDiscount = 10
                },
            ]
        }
    ];

    private static List<Order> Orders =>
    [
        new()
        {
            Id = 1,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2023, 12, 20, 6, 0, 0, TimeSpan.Zero),
            Status = "Delivered",
            PaymentReceived = true,
            OrderRecords =
            [
                new()
                {
                    Id = 1,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 2,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        },
        new()
        {
            Id = 2,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2024, 3, 20, 6, 0, 0, TimeSpan.Zero),
            Status = "Delivered",
            PaymentReceived = true,
            OrderRecords =
            [
                new()
                {
                    Id = 3,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 4,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        },
        new()
        {
            Id = 3,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2024, 4, 1, 6, 0, 0, TimeSpan.Zero),
            Status = "Delivered",
            PaymentReceived = true,
            OrderRecords =
            [
                new()
                {
                    Id = 5,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 6,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        },
        new()
        {
            Id = 4,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2024, 4, 2, 6, 0, 0, TimeSpan.Zero),
            Status = "Delivered",
            PaymentReceived = true,
            OrderRecords =
            [
                new()
                {
                    Id = 7,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 8,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        },
        new()
        {
            Id = 5,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2024, 4, 3, 6, 0, 0, TimeSpan.Zero),
            Status = "Delivered",
            PaymentReceived = true,
            OrderRecords =
            [
                new()
                {
                    Id = 9,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 10,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        },
        new()
        {
            Id = 6,
            UserId = "1234-5678-9012-3456",
            OrderDate = new DateTimeOffset(2024, 4, 4, 6, 0, 0, TimeSpan.Zero),
            Status = "Created",
            PaymentReceived = false,
            OrderRecords =
            [
                new()
                {
                    Id = 11,
                    ProductName = "Product 1",
                    ProductInstanceId = 1,
                    Price = 75,
                    Quantity = 1,
                    Discount = 10
                },
                new()
                {
                    Id = 12,
                    ProductName = "Product 1",
                    ProductInstanceId = 2,
                    Price = 55,
                    Quantity = 1,
                    Discount = 5
                }
            ]
        }
    ];

    private static List<EmailLog> EmailLogs =>
    [
        new()
        {
            Id = 1,
            RecipientEmail = "test1@example.com",
            SendAt = new DateTimeOffset(2024, 1, 10, 6, 0, 0, TimeSpan.Zero),
            Subject = "Subject Test"
        },
        new()
        {
            Id = 2,
            RecipientEmail = "test2@example.com",
            SendAt = new DateTimeOffset(2024, 2, 10, 6, 0, 0, TimeSpan.Zero),
            Subject = "Subject New"
        },
        new()
        {
            Id = 3,
            RecipientEmail = "user@email.com",
            SendAt = new DateTimeOffset(2024, 3, 10, 6, 0, 0, TimeSpan.Zero),
            Subject = "Another topic"
        }
    ];
}
