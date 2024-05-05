using HM.BLL.Interfaces;
using HM.BLL.Models.Statistics;
using HM.DAL.Entities;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class StatisticsControllerTests
{
    private readonly IStatisticsService _statisticsService;
    private readonly StatisticsController _statisticsController;
    public StatisticsControllerTests()
    {
        _statisticsService = Substitute.For<IStatisticsService>();
        _statisticsController = new StatisticsController(_statisticsService);
    }
    [Fact]
    public async Task GetProductsStatistics_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_statisticsController);
        _statisticsService.GetProductStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), CancellationToken.None)
            .Returns([]);

        ActionResult<IEnumerable<ProductStatisticDto>> response = await _statisticsController
            .GetProductsStatistics(1, 1, 1);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategoryGroupsStatistics_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_statisticsController);
        _statisticsService.GetCategoryStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<CategoryStatisticDto>> response = await _statisticsController
            .GetCategoryGroupsStatistics(1);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategoriesStatistics_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_statisticsController);
        _statisticsService.GetCategoryStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<CategoryStatisticDto>> response = await _statisticsController
            .GetCategoriesStatistics(1);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetOrdersStatistics_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_statisticsController);
        _statisticsService.GetOrderStatisticsAsync(Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<OrderStatisticDto>> response = await _statisticsController
            .GetOrdersStatistics();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetEmailLogs_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_statisticsController);
        _statisticsService.GetEmailLogsAsync(Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<EmailLog>> response = await _statisticsController.GetEmailLogs();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
}
