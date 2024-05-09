using HM.BLL.Interfaces;
using HM.BLL.Models.Statistics;
using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController(
    IStatisticsService statisticsService,
    IExcelHelper excelHelper
    ) : Controller
{
    /// <summary>
    /// Allows administrators to get products statistics.
    /// </summary>
    /// <param name="categoryGroupId">Optional. Allows to get statistics for the category group with the 
    /// specified ID. Is Applied only if <paramref name="categoryId"/> or <paramref name="productId"/>
    /// are not set.</param>
    /// <param name="categoryId">Optional. Allows to get statistics for the category with the 
    /// specified ID. Is Applied only if <paramref name="productId"/> is not set.</param>
    /// <param name="productId">Optional. Allows to get statistics for the product with the 
    /// specified ID.</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="sortByDateFirst">Optional. If set to true statistics is sorted firstly by date. Otherwise
    /// it is sort firstly by product id.</param>
    /// <param name="sortByDateAsc">Optional. If set to true sorting by date is done in ascending order,
    /// otherwise - in descending order.</param>
    /// <param name="sortByProductAsc">Optional. If set to true sorting by product is done in ascending order,
    /// otherwise - in descending order.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("products")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProductStatisticDto>>> GetProductsStatistics(
        int? categoryGroupId, int? categoryId, int? productId, DateOnly? fromDate = null,
        DateOnly? toDate = null, bool yearly = false, bool monthly = false, bool daily = false,
        bool sortByDateFirst = true, bool sortByDateAsc = true, bool sortByProductAsc = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await statisticsService.GetProductStatisticsAsync(productId, categoryId,
            categoryGroupId, fromDate, toDate, yearly, monthly, daily, sortByDateFirst, sortByDateAsc,
            sortByProductAsc, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to get an Excel file with the products statistics.
    /// </summary>
    /// <param name="categoryGroupId">Optional. Allows to get statistics for the category group with the 
    /// specified ID. Is Applied only if <paramref name="categoryId"/> or <paramref name="productId"/>
    /// are not set.</param>
    /// <param name="categoryId">Optional. Allows to get statistics for the category with the 
    /// specified ID. Is Applied only if <paramref name="productId"/> is not set.</param>
    /// <param name="productId">Optional. Allows to get statistics for the product with the 
    /// specified ID.</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="sortByDateFirst">Optional. If set to true statistics is sorted firstly by date. Otherwise
    /// it is sort firstly by product id.</param>
    /// <param name="sortByDateAsc">Optional. If set to true sorting by date is done in ascending order,
    /// otherwise - in descending order.</param>
    /// <param name="sortByProductAsc">Optional. If set to true sorting by product is done in ascending order,
    /// otherwise - in descending order.</param>
    /// <param name="includeInstances">Optional. If set to true the returned file will contain statistics
    /// for each product instances (only for the product that contain two or more product instances).</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("products/excel")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportProductStatisticsToExcel(
        int? categoryGroupId, int? categoryId, int? productId, DateOnly? fromDate = null,
        DateOnly? toDate = null, bool yearly = false, bool monthly = false, bool daily = false,
        bool sortByDateFirst = true, bool sortByDateAsc = true, bool sortByProductAsc = true,
        bool includeInstances = true, CancellationToken cancellationToken = default)
    {
        IEnumerable<ProductStatisticDto> products = await statisticsService.GetProductStatisticsAsync(
            productId, categoryId, categoryGroupId, fromDate, toDate, yearly, monthly, daily,
            sortByDateFirst, sortByDateAsc, sortByProductAsc, cancellationToken);
        MemoryStream stream = excelHelper.WriteProductStatistics(products, includeInstances, cancellationToken);
        string name = IncludeTimeIntoFileName(
            $"Статистика_товар{(productId == null ? "ів" : $"у_{productId}")}" +
            $"{(categoryGroupId == null ? "" : $"_категорії_{categoryGroupId}")}" +
            $"{(categoryId == null ? "" : $"_підкатегорії_{categoryId}")}",
            fromDate, toDate);
        return File(stream, excelHelper.ExcelMimeType, name);
    }

    /// <summary>
    /// Allows administrators to get category groups statistics.
    /// </summary>
    /// <param name="categoryGroupId">Allows to get statistics for the category group with the 
    /// specified ID</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the categories statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("categoryGroups/{categoryGroupId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CategoryStatisticDto>>> GetCategoryGroupsStatistics(
        int categoryGroupId, DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false,
        bool monthly = false, bool daily = false, CancellationToken cancellationToken = default)
    {
        return Ok(await statisticsService.GetCategoryStatisticsAsync(categoryGroupId,
            null, fromDate, toDate, yearly, monthly, daily, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to get an Excel file with the category groups statistics.
    /// </summary>
    /// <param name="categoryGroupId">Allows to get statistics for the category group with the 
    /// specified ID</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("categoryGroups/{categoryGroupId}/excel")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportCategoryGroupStatisticsToExcel(
        int categoryGroupId, DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false,
        bool monthly = false, bool daily = false, CancellationToken cancellationToken = default)
    {
        IEnumerable<CategoryStatisticDto> categoryGroups = await statisticsService
            .GetCategoryStatisticsAsync(categoryGroupId, null, fromDate, toDate,
            yearly, monthly, daily, cancellationToken);
        MemoryStream stream = excelHelper.WriteCategoryStatistics(categoryGroups, cancellationToken);
        string name = IncludeTimeIntoFileName($"Статистика_категорії_{categoryGroupId}", fromDate, toDate);
        return File(stream, excelHelper.ExcelMimeType, name);
    }

    /// <summary>
    /// Allows administrators to get category statistics.
    /// </summary>
    /// <param name="categoryId">Allows to get statistics for the category with the 
    /// specified ID</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the categories statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("categories/{categoryId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CategoryStatisticDto>>> GetCategoriesStatistics(
        int categoryId, DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false,
        bool monthly = false, bool daily = false, CancellationToken cancellationToken = default)
    {
        return Ok(await statisticsService.GetCategoryStatisticsAsync(null,
            categoryId, fromDate, toDate, yearly, monthly, daily, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to get an Excel file with the category statistics.
    /// </summary>
    /// <param name="categoryId">Allows to get statistics for the category with the 
    /// specified ID</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("categories/{categoryId}/excel")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportCategoriesStatisticsToExcel(
        int categoryId, DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false,
        bool monthly = false, bool daily = false, CancellationToken cancellationToken = default)
    {
        IEnumerable<CategoryStatisticDto> categoryGroups = await statisticsService
            .GetCategoryStatisticsAsync(null, categoryId, fromDate, toDate,
            yearly, monthly, daily, cancellationToken);
        MemoryStream stream = excelHelper.WriteCategoryStatistics(categoryGroups, cancellationToken);
        string name = IncludeTimeIntoFileName($"Статистика_підкатегорії_{categoryId}", fromDate, toDate);
        return File(stream, excelHelper.ExcelMimeType, name);
    }

    /// <summary>
    /// Allows administrators to get orders statistics.
    /// </summary>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="includeUnpaid">Optional. If set to true includes the orders for which payment has not been received.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the orders statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("orders")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderStatisticDto>>> GetOrdersStatistics(
        DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false, bool monthly = false,
        bool daily = false, bool includeUnpaid = false, CancellationToken cancellationToken = default)
    {
        return Ok(await statisticsService.GetOrderStatisticsAsync(
            fromDate, toDate, yearly, monthly, daily, includeUnpaid, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to get an Excel file the orders statistics.
    /// </summary>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="includeUnpaid">Optional. If set to true includes the orders for which payment has not been received.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("orders/excel")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportOrdersStatisticsToExcel(
        DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false, bool monthly = false,
        bool daily = false, bool includeUnpaid = false, CancellationToken cancellationToken = default)
    {
        IEnumerable<OrderStatisticDto> orders = await statisticsService.GetOrderStatisticsAsync(
            fromDate, toDate, yearly, monthly, daily, includeUnpaid, cancellationToken);
        MemoryStream stream = excelHelper.WriteOrderStatistics(orders, cancellationToken);
        string name = IncludeTimeIntoFileName($"Статистика_замовлень{(includeUnpaid ? "_(включаючи_неоплачені)" : "")}",
            fromDate, toDate);
        return File(stream, excelHelper.ExcelMimeType, name);
    }

    /// <summary>
    /// Allows administrators to get list of emails that was sent.
    /// </summary>
    /// <param name="recipientEmail">Optional. Allows to filter emails by recipient.</param>
    /// <param name="subject">Optional. Allows to filter emails by subject.</param>
    /// <param name="startDateTime">Optional. Allows to filter emails after specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="endDateTime">Optional. Allows to filter emails before specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the list of email that was sent.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("emails")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EmailLog>>> GetEmailLogs(
        string? recipientEmail = null, string? subject = null,
        DateOnly? startDateTime = null, DateOnly? endDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await statisticsService.GetEmailLogsAsync(
            recipientEmail, subject, startDateTime, endDateTime, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to an Excel file with emails that was sent.
    /// </summary>
    /// <param name="recipientEmail">Optional. Allows to filter emails by recipient.</param>
    /// <param name="subject">Optional. Allows to filter emails by subject.</param>
    /// <param name="startDateTime">Optional. Allows to filter emails after specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="endDateTime">Optional. Allows to filter emails before specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("emails/excel")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ExportEmailLogsToExcel(
        string? recipientEmail = null, string? subject = null,
        DateOnly? startDateTime = null, DateOnly? endDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<EmailLog> emailLogs = await statisticsService.GetEmailLogsAsync(
            recipientEmail, subject, startDateTime, endDateTime, cancellationToken);
        MemoryStream stream = excelHelper.WriteEmailLogs(emailLogs, cancellationToken);
        string name = IncludeTimeIntoFileName("Відправлені_листи", startDateTime, endDateTime);
        return File(stream, excelHelper.ExcelMimeType, name);
    }

    private static string IncludeTimeIntoFileName(string name, DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (dateFrom.HasValue && dateTo.HasValue)
        {
            name += $"_{dateFrom}-{dateTo}";
        }
        else if (dateFrom != null)
        {
            name += $"_після_{dateFrom}";
        }
        else if (dateTo != null)
        {
            name += $"_до_{dateTo}";
        }
        return name + ".xlsx";
    }
}
