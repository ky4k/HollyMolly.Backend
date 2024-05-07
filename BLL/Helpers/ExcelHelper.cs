using ClosedXML.Excel;
using HM.BLL.Interfaces;
using HM.BLL.Models.Statistics;
using HM.DAL.Entities;

namespace HM.BLL.Helpers;

public class ExcelHelper : IExcelHelper
{
    private const string excelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string ExcelMimeType => excelMimeType;

    public MemoryStream WriteProductStatistics(IEnumerable<ProductStatisticDto> productStatisticsDto,
        bool includeInstances, CancellationToken cancellationToken)
    {
        XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Товари");
        string[] columns = [ "Період", "ID товару", "Назва товару", "Перегляди", "Покупки",
            "Покупок на перегляд", "Загальна вартість проданих товарів", "Додано в список бажань",
            "Коментарі", "Рейтинг" ];
        WriteHeader(worksheet, columns);
        int row = 2;
        foreach (ProductStatisticDto productStatisticDto in productStatisticsDto)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteProductRow(worksheet, row, productStatisticDto);
            row++;
            if (includeInstances && productStatisticDto.InstancesStatistics.Count > 1)
            {
                foreach (ProductInstanceStatisticDto productInstanceStatisticDto in productStatisticDto.InstancesStatistics)
                {
                    WriteProductInstanceRow(worksheet, row, productInstanceStatisticDto);
                    row++;
                }
            }
        }
        StylizeTable(worksheet, 10);
        return SaveAsMemoryStreamAsync(workbook);
    }

    public MemoryStream WriteCategoryStatistics(IEnumerable<CategoryStatisticDto> categoryStatisticsDto, CancellationToken cancellationToken)
    {
        XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Категорії");
        string[] columns = [ "Період", "ID категорії", "Назва категорії", "Перегляди товарів",
            "Покупки товарів", "Загальна вартість проданих товарів", "Товарів додано в список бажань",
            "Коментарі" ];
        WriteHeader(worksheet, columns);
        int row = 2;
        foreach (CategoryStatisticDto categoryStatisticDto in categoryStatisticsDto)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteCategoryRow(worksheet, row, categoryStatisticDto);
            row++;
        }
        StylizeTable(worksheet, 8);
        return SaveAsMemoryStreamAsync(workbook);
    }
    public MemoryStream WriteOrderStatistics(IEnumerable<OrderStatisticDto> orderStatisticsDto, CancellationToken cancellationToken)
    {
        XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Замовлення");
        string[] columns = [ "Період", "Кількість замовлень", "Вартість до знижки", "Сума знижки",
            "Середня вартість замовлення", "Загальна вартість" ];
        WriteHeader(worksheet, columns);
        int row = 2;
        foreach (OrderStatisticDto orderStatisticDto in orderStatisticsDto)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteOrderRow(worksheet, row, orderStatisticDto);
            row++;
        }
        StylizeTable(worksheet, 6);
        return SaveAsMemoryStreamAsync(workbook);
    }
    public MemoryStream WriteEmailLogs(IEnumerable<EmailLog> emailLogs, CancellationToken cancellationToken)
    {
        XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Emails");
        string[] columns = ["Id", "Час відправки", "Пошта одержувача", "Тема"];
        WriteHeader(worksheet, columns);
        int row = 2;
        foreach (EmailLog emailLog in emailLogs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteEmailLogRow(worksheet, row, emailLog);
            row++;
        }
        StylizeTable(worksheet, 4);
        return SaveAsMemoryStreamAsync(workbook);
    }

    private static void WriteHeader(IXLWorksheet worksheet, string[] columnNames)
    {
        for (int i = 0; i < columnNames.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = columnNames[i];
        }
    }
    private static void WriteProductRow(IXLWorksheet worksheet, int row, ProductStatisticDto productStatisticDto)
    {
        worksheet.Cell(row, 1).Value = GetPeriod(
                productStatisticDto.Year, productStatisticDto.Month, productStatisticDto.Day);
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 2).Value = productStatisticDto.ProductId;
        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 3).Value = productStatisticDto.ProductName;
        worksheet.Cell(row, 4).Value = productStatisticDto.NumberViews;
        worksheet.Cell(row, 5).Value = productStatisticDto.NumberPurchases;
        worksheet.Cell(row, 6).Value = Math.Round(productStatisticDto.ConversionRate, 4);
        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#0.0000";
        worksheet.Cell(row, 7).Value = productStatisticDto.TotalRevenue;
        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#0.00";
        worksheet.Cell(row, 8).Value = productStatisticDto.NumberWishListAddition;
        worksheet.Cell(row, 9).Value = productStatisticDto.NumberReviews;
        worksheet.Cell(row, 10).Value = productStatisticDto.Rating;
        worksheet.Cell(row, 10).Style.NumberFormat.Format = "#0.00";
    }
    private static void WriteProductInstanceRow(IXLWorksheet worksheet, int row,
        ProductInstanceStatisticDto productInstanceStatisticDto)
    {
        worksheet.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Cell(row, 2).Value = "в т.ч.:";
        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 3).Value = GetProductInstanceName(productInstanceStatisticDto);
        worksheet.Cell(row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Cell(row, 5).Value = productInstanceStatisticDto.NumberOfPurchases;
        worksheet.Cell(row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Cell(row, 7).Value = productInstanceStatisticDto.TotalRevenue;
        worksheet.Cell(row, 7).Style.NumberFormat.Format = "#0.00";
        worksheet.Cell(row, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Cell(row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Cell(row, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
    private static string GetProductInstanceName(ProductInstanceStatisticDto productInstanceStatisticDto)
    {
        return "  " +
            $"{productInstanceStatisticDto.SKU ?? ""} " +
            $"{productInstanceStatisticDto.Color ?? ""} " +
            $"{productInstanceStatisticDto.Size ?? ""} " +
            $"{productInstanceStatisticDto.Material ?? ""}";
    }
    private static void WriteCategoryRow(IXLWorksheet worksheet, int row, CategoryStatisticDto categoryStatisticDto)
    {
        worksheet.Cell(row, 1).Value = GetPeriod(
                categoryStatisticDto.Year, categoryStatisticDto.Month, categoryStatisticDto.Day);
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 2).Value = categoryStatisticDto.CategoryId;
        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 3).Value = categoryStatisticDto.CategoryName;
        worksheet.Cell(row, 4).Value = categoryStatisticDto.NumberProductViews;
        worksheet.Cell(row, 5).Value = categoryStatisticDto.NumberPurchases;
        worksheet.Cell(row, 6).Value = categoryStatisticDto.TotalRevenue;
        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#0.00";
        worksheet.Cell(row, 7).Value = categoryStatisticDto.WishListAddition;
        worksheet.Cell(row, 8).Value = categoryStatisticDto.NumberReviews;
    }
    private static void WriteOrderRow(IXLWorksheet worksheet, int row, OrderStatisticDto orderStatisticDto)
    {
        worksheet.Cell(row, 1).Value = GetPeriod(
                orderStatisticDto.Year, orderStatisticDto.Month, orderStatisticDto.Day);
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 2).Value = orderStatisticDto.NumberOfOrders;
        worksheet.Cell(row, 3).Value = orderStatisticDto.TotalCostBeforeDiscount;
        worksheet.Cell(row, 3).Style.NumberFormat.Format = "#0.00";
        worksheet.Cell(row, 4).Value = orderStatisticDto.TotalDiscount;
        worksheet.Cell(row, 4).Style.NumberFormat.Format = "#0.00";
        worksheet.Cell(row, 5).Value = Math.Round(orderStatisticDto.AverageOrderCost, 2);
        worksheet.Cell(row, 6).Value = orderStatisticDto.TotalCost;
        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#0.00";
    }
    private static void WriteEmailLogRow(IXLWorksheet worksheet, int row, EmailLog emailLog)
    {
        worksheet.Cell(row, 1).Value = emailLog.Id;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 2).Value = emailLog.SendAt.ToString("yyyy-MM-dd");
        worksheet.Cell(row, 3).Value = emailLog.RecipientEmail;
        worksheet.Cell(row, 4).Value = emailLog.Subject;
    }

    private static string GetPeriod(int? year, int? month, int? day)
    {
        string period = year.HasValue ? year.Value.ToString() : "Весь час";
        period += month.HasValue ? $"-{month:D2}" : "";
        period += day.HasValue ? $"-{day:D2}" : "";
        return period;
    }
    private static void StylizeTable(IXLWorksheet worksheet, int lastColumn)
    {
        worksheet.Row(1).Cells(1, lastColumn).Style.Alignment.WrapText = true;
        worksheet.Row(1).Cells(1, lastColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Row(1).Cells(1, lastColumn).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Columns(1, lastColumn).AdjustToContents(2, 10, 40);
        worksheet.Cells(true).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static MemoryStream SaveAsMemoryStreamAsync(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
