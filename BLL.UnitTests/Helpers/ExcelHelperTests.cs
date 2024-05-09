using HM.BLL.Helpers;
using HM.BLL.Models.Statistics;
using HM.DAL.Entities;

namespace HM.BLL.UnitTests.Helpers;

public class ExcelHelperTests
{
    private readonly ExcelHelper _excelHelper;
    public ExcelHelperTests()
    {
        _excelHelper = new ExcelHelper();
    }
    [Fact]
    public void ExcelMimeType_ShouldBeCorrect()
    {
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _excelHelper.ExcelMimeType);
    }
    [Fact]
    public void WriteProductStatistics_ShouldReturnMemoryStreamWithoutInstances_WhenIncludeInstancesIsFalse()
    {
        List<ProductStatisticDto> productStatisticsDto =
        [
            new()
            {
                ProductId = 1,
                Year = 2024,
                Month = 5,
                InstancesStatistics =
                [
                    new()
                    {
                        SKU = "Long SKY adds additional bytes to the result file."
                    }
                ]
            }
        ];

        MemoryStream stream = _excelHelper.WriteProductStatistics(
            productStatisticsDto, false, CancellationToken.None);

        Assert.Equal(6703, stream.Length);
    }
    [Fact]
    public void WriteProductStatistics_ShouldReturnMemoryStreamAndNotIncludeInstances_WhenProductContainOnlyOneInstance()
    {
        List<ProductStatisticDto> productStatisticsDto =
        [
            new()
            {
                ProductId = 1,
                Year = 2024,
                Month = 5,
                InstancesStatistics =
                [
                    new()
                    {
                        SKU = "Long SKY adds additional bytes to the result file."
                    }
                ]
            }
        ];

        MemoryStream stream = _excelHelper.WriteProductStatistics(
            productStatisticsDto, true, CancellationToken.None);

        Assert.Equal(6703, stream.Length);
    }
    [Fact]
    public void WriteProductStatistics_ShouldReturnMemoryStream_WithInstances()
    {
        List<ProductStatisticDto> productStatisticsDto =
        [
            new()
            {
                ProductId = 1,
                InstancesStatistics =
                [
                    new()
                    {
                        SKU = "First. Long SKY adds additional bytes to the result file."
                    },
                    new()
                    {
                        SKU = "Second. Long SKY adds additional bytes to the result file."
                    }
                ]
            }
        ];

        MemoryStream stream = _excelHelper.WriteProductStatistics(
            productStatisticsDto, true, CancellationToken.None);

        Assert.Equal(6906, stream.Length);
    }
    [Fact]
    public void WriteCategoryStatistics_ShouldReturnMemoryStream()
    {
        CategoryStatisticDto categoryStatisticDto = new()
        {
            CategoryId = 1,
            Year = 2024,
            Month = 4,
            Day = 1
        };

        MemoryStream stream = _excelHelper.WriteCategoryStatistics([categoryStatisticDto], CancellationToken.None);

        Assert.True(stream.Length > 1000);
    }
    [Fact]
    public void WriteOrderStatistics_ShouldReturnMemoryStream()
    {
        OrderStatisticDto orderStatisticDto = new()
        {
            Year = 2024,
            Month = 4,
            Day = 1
        };
        MemoryStream stream = _excelHelper.WriteOrderStatistics([orderStatisticDto], CancellationToken.None);

        Assert.True(stream.Length > 1000);
    }
    [Fact]
    public void WriteEmailLogs_ShouldReturnMemoryStream()
    {
        EmailLog emailLog = new()
        {
            Id = 1
        };
        MemoryStream stream = _excelHelper.WriteEmailLogs([emailLog], CancellationToken.None);

        Assert.True(stream.Length > 1000);
    }
}
