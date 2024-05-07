using HM.BLL.Models.Statistics;
using HM.DAL.Entities;

namespace HM.BLL.Interfaces;

public interface IExcelHelper
{
    string ExcelMimeType { get; }
    MemoryStream WriteProductStatistics(IEnumerable<ProductStatisticDto> productStatisticsDto,
        bool includeInstances, CancellationToken cancellationToken);

    MemoryStream WriteCategoryStatistics(IEnumerable<CategoryStatisticDto> categoryStatisticsDto,
        CancellationToken cancellationToken);

    MemoryStream WriteOrderStatistics(IEnumerable<OrderStatisticDto> orderStatisticsDto, CancellationToken cancellationToken);
    MemoryStream WriteEmailLogs(IEnumerable<EmailLog> emailLogs, CancellationToken cancellationToken);
}
