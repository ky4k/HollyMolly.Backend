using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Orders;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(
    IOrderService orderService,
    IStatisticsService statisticsService,
    IEmailService emailService
    ) : ControllerBase
{
    /// <summary>
    /// Allows administrators and managers to retrieve all orders.
    /// </summary>
    /// <param name="statuses">Optional. If set only orders with the specified statuses will be returned.</param>
    /// <param name="fromDate">Optional. If set only orders after this date will be returned.</param>
    /// <param name="toDate">Optional. If set only orders before this date will be returned.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the list of all orders.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the orders.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(
        [FromQuery]IEnumerable<string>? statuses = null,
        DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await orderService.GetOrdersAsync(null, statuses, fromDate, toDate, cancellationToken));
    }

    /// <summary>
    /// Allows a registered user to retrieve all their orders.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the list of all orders.</response>
    /// <response code="400">Indicates that the request is invalid or incomplete.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    [Authorize]
    [Route("myOrders")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("Token does not contain a userId");
        }
        return Ok(await orderService.GetOrdersAsync(userId, null, null, null, cancellationToken));
    }

    /// <summary>
    /// Allows administrators and managers to retrieve an order by its ID.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the order with the specified ID.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to access the order.</response>
    /// <response code="404">Indicates that no order with the specified ID exists.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("{orderId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(int orderId, CancellationToken cancellationToken)
    {
        OrderDto? orderDto = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        return orderDto == null ? NotFound() : Ok(orderDto);
    }

    /// <summary>
    /// Allows to create a new order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the order has been successfully created and returns the created order.</response>
    /// <response code="400">Indicates that the request to create the order is invalid or incomplete.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    [Authorize]
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> CreateOrder(OrderCreateDto order,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        return await CreateOrder(userId, order, cancellationToken, sendEmail);
    }

    /// <summary>
    /// Allows administrators and managers to create a new order for the user.
    /// </summary>
    /// <param name="userId">The Id of the user to create a new order for.</param>
    /// <param name="order">The order to create.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the order has been successfully created and returns the created order.</response>
    /// <response code="400">Indicates that the request to create the order is invalid or incomplete.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("{userId}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> CreateOrder(string userId, OrderCreateDto order,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        OperationResult<OrderDto> result = await orderService.CreateOrderAsync(order, userId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            await emailService.SendOrderCreatedEmailAsync(result.Payload!, cancellationToken);
        }

        await statisticsService.AddToProductNumberPurchasesAsync(result.Payload!);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = result.Payload!.Id }, result.Payload);
    }

    /// <summary>
    /// Allows to update an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="updatedOrder">The updated order information.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the order has been successfully updated and returns the updated order.</response>
    /// <response code="400">Indicates that the request to update the order is invalid or incomplete.</response>
    /// <response code="401">Indicates that the user is not authorized to update the order.</response>
    /// <response code="403">Indicates that the user does not have permission to update the order.</response>
    /// <remarks>Allowed order statuses: "Created", "Payment Received", "Processing", "Shipped", "Delivered", "Cancelled"</remarks>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("{orderId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int orderId, OrderUpdateDto updatedOrder,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        OperationResult<OrderDto> result = await orderService.UpdateOrderAsync(orderId, updatedOrder, cancellationToken);
        if (!result.Succeeded || result.Payload == null)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            await emailService.SendOrderStatusUpdatedEmailAsync(result.Payload, cancellationToken);
        }
        return Ok(result.Payload);
    }
}
