using HM.BLL.Interfaces;
using HM.BLL.Models.Statistics;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Authorize(Roles = DefaultRoles.Administrator)]
[Route("api/[controller]")]
[ApiController]
public class StatisticsController(
    IStatisticsService statisticsService
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
    /// <response code="200">Returns the products statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Route("products")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProductStatisticDto>>> GetProductsStatistics(
        int? categoryGroupId, int? categoryId, int? productId, DateOnly? fromDate = null,
        DateOnly? toDate = null, bool yearly = false, bool monthly = false, bool daily = false,
        bool sortByDateFirst = true, bool sortByDateAsc = true, bool sortByProductAsc = true)
    {
        return Ok(await statisticsService.GetProductStatisticsAsync(productId, categoryId,
            categoryGroupId, fromDate, toDate, yearly, monthly, daily, sortByDateFirst, sortByDateAsc,
            sortByProductAsc, Request.HttpContext.RequestAborted));
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
    /// <response code="200">Returns the categories statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Route("categoryGroups/{categoryGroupId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryStatisticDto>> GetCategoryGroupsStatistics(
        int categoryGroupId, DateOnly? fromDate = null, DateOnly? toDate = null,
        bool yearly = false, bool monthly = false, bool daily = false)
    {
        return Ok(await statisticsService.GetCategoryStatisticsAsync(categoryGroupId,
            null, fromDate, toDate, yearly, monthly, daily, Request.HttpContext.RequestAborted));
    }

    /// <summary>
    /// Allows administrators to get category groups statistics.
    /// </summary>
    /// <param name="categoryId">Allows to get statistics for the category with the 
    /// specified ID</param>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <response code="200">Returns the categories statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Route("categories/{categoryId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryStatisticDto>> GetCategoriesStatistics(
        int categoryId, DateOnly? fromDate = null, DateOnly? toDate = null,
        bool yearly = false, bool monthly = false, bool daily = false)
    {
        return Ok(await statisticsService.GetCategoryStatisticsAsync(null,
            categoryId, fromDate, toDate, yearly, monthly, daily, Request.HttpContext.RequestAborted));
    }

    /// <summary>
    /// Allows administrators to get categories statistics.
    /// </summary>
    /// <param name="fromDate">Optional. Allows to get statistics after the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="toDate">Optional. Allows to get statistics before the specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="yearly">Optional. If set to true returns statistics for every year.</param>
    /// <param name="monthly">Optional. If set to true returns statistics for every month.</param>
    /// <param name="daily">Optional. If set to true returns statistics for every day.</param>
    /// <param name="includeUnpaid">Optional. If set to true includes the orders for which payment has not been received.</param>
    /// <response code="200">Returns the orders statistics.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Route("orders")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderStatisticDto>>> GetOrdersStatistics(
        DateOnly? fromDate = null, DateOnly? toDate = null, bool yearly = false, bool monthly = false,
        bool daily = false, bool includeUnpaid = false)
    {
        return Ok(await statisticsService.GetOrderStatisticsAsync(
            fromDate, toDate, yearly, monthly, daily, includeUnpaid, Request.HttpContext.RequestAborted));
    }

    /// <summary>
    /// Allows administrators to get list of emails that was sent.
    /// </summary>
    /// <param name="recipientEmail">Optional. Allows to filter emails by recipient.</param>
    /// <param name="subject">Optional. Allows to filter emails by subject.</param>
    /// <param name="startDateTime">Optional. Allows to filter emails after specified date. Date should be in format YYYY-MM-DD.</param>
    /// <param name="endDateTime">Optional. Allows to filter emails before specified date. Date should be in format YYYY-MM-DD.</param>
    /// <response code="200">Returns the list of email that was sent.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Route("emails")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderStatisticDto>>> GetEmailLogs(
        string? recipientEmail = null, string? subject = null,
        DateOnly? startDateTime = null, DateOnly? endDateTime = null)
    {
        return Ok(await statisticsService.GetEmailLogsAsync(
            recipientEmail, subject, startDateTime, endDateTime, Request.HttpContext.RequestAborted));
    }
}
