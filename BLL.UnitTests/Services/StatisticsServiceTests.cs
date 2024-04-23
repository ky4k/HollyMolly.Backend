using HM.BLL.Models.Statistics;
using HM.BLL.Services;
using HM.BLL.UnitTests.Helpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

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

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }
    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.CategoryGroups.AddRangeAsync(CategoryGroups);
        await context.Categories.AddRangeAsync(Categories);
        await context.Products.AddRangeAsync(Products);
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
                    NumberViews = 50,
                    NumberFeedbacks = 25,
                    NumberWishlistAdditions = 15,
                },
                new()
                {
                    Id = 23,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 50,
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
}
