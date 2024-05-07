using HM.BLL.Interfaces;
using HM.BLL.Models.Statistics;
using HM.DAL.Entities;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace WebAPI.UnitTests.Controllers;

public class StatisticsControllerTests
{
    private readonly IStatisticsService _statisticsService;
    private readonly IExcelHelper _excelHelper;
    private readonly StatisticsController _statisticsController;
    public StatisticsControllerTests()
    {
        _statisticsService = Substitute.For<IStatisticsService>();
        _excelHelper = Substitute.For<IExcelHelper>();
        _excelHelper.ExcelMimeType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _statisticsController = new StatisticsController(_statisticsService, _excelHelper);
    }
    [Fact]
    public async Task GetProductsStatistics_ShouldReturnOkResult()
    {
        _statisticsService.GetProductStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<ProductStatisticDto>> response = await _statisticsController
            .GetProductsStatistics(1, 1, 1);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task ExportProductStatisticsToExcel_ShouldReturnFileStreamResult_WhenCalledWithParameters()
    {
        _statisticsService.GetProductStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteProductStatistics(
            Arg.Any<IEnumerable<ProductStatisticDto>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController
            .ExportProductStatisticsToExcel(1, 1, 1, new DateOnly(2024, 4, 1), new DateOnly(2024, 12, 1));

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task ExportProductStatisticsToExcel_ShouldReturnFileStreamResult_WhenCalledWithoutParameters()
    {
        _statisticsService.GetProductStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteProductStatistics(
            Arg.Any<IEnumerable<ProductStatisticDto>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController.ExportProductStatisticsToExcel(null, null, null);

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task GetCategoryGroupsStatistics_ShouldReturnOkResult()
    {
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
    public async Task ExportCategoryGroupStatisticsToExcel_ShouldReturnFileStreamResult()
    {
        _statisticsService.GetCategoryStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteCategoryStatistics(Arg.Any<IEnumerable<CategoryStatisticDto>>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController
            .ExportCategoryGroupStatisticsToExcel(1, new DateOnly(2024, 4, 1));

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task GetCategoriesStatistics_ShouldReturnOkResult()
    {
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
    public async Task ExportCategoriesStatisticsToExcel_ShouldReturnFileStreamResult()
    {
        _statisticsService.GetCategoryStatisticsAsync(Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteCategoryStatistics(Arg.Any<IEnumerable<CategoryStatisticDto>>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController
            .ExportCategoriesStatisticsToExcel(1, null, new DateOnly(2024, 12, 1));

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task GetOrdersStatistics_ShouldReturnOkResult()
    {
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
    public async Task ExportOrdersStatisticsToExcel_ShouldReturnFileStreamResult_WhenIncludeUnpaidIsTrue()
    {
        _statisticsService.GetOrderStatisticsAsync(Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteOrderStatistics(Arg.Any<IEnumerable<OrderStatisticDto>>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController.ExportOrdersStatisticsToExcel(includeUnpaid: true);

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task ExportOrdersStatisticsToExcel_ShouldReturnFileStreamResult_WhenIncludeUnpaidIsFalse()
    {
        _statisticsService.GetOrderStatisticsAsync(Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteOrderStatistics(Arg.Any<IEnumerable<OrderStatisticDto>>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController.ExportOrdersStatisticsToExcel(includeUnpaid: false);

        Assert.IsType<FileStreamResult>(response);
    }
    [Fact]
    public async Task GetEmailLogs_ShouldReturnOkResult()
    {
        _statisticsService.GetEmailLogsAsync(Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<EmailLog>> response = await _statisticsController.GetEmailLogs();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task ExportEmailLogsToExcel_ShouldReturnFileStreamResult()
    {
        _statisticsService.GetEmailLogsAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excelHelper.WriteEmailLogs(Arg.Any<IEnumerable<EmailLog>>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream());

        ActionResult response = await _statisticsController.ExportEmailLogsToExcel();

        Assert.IsType<FileStreamResult>(response);
    }
}
