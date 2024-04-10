﻿using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(
    IOrderService orderService
    ) : ControllerBase
{
    /// <summary>
    /// Allows administrators and managers to retrieve all orders.
    /// </summary>
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
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetOrdersAsync(null, cancellationToken));
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
        if(userId == null)
        {
            return BadRequest("Token does not contain a userId");
        }
        return Ok(await orderService.GetOrdersAsync(userId, cancellationToken));
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
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the order has been successfully created and returns the created order.</response>
    /// <response code="400">Indicates that the request to create the order is invalid or incomplete.</response>
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(OrderCreateDto order, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        OperationResult<OrderDto> result = await orderService.CreateOrderAsync(order, userId, cancellationToken);
        return result.Succeeded && result.Payload != null
            ? CreatedAtAction(nameof(GetOrderById), new { orderId = result.Payload.Id }, result.Payload)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows to update an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to update.</param>
    /// <param name="updatedOrder">The updated order information.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the order has been successfully updated and returns the updated order.</response>
    /// <response code="400">Indicates that the request to update the order is invalid or incomplete.</response>
    /// <response code="401">Indicates that the user is not authorized to update the order.</response>
    /// <response code="403">Indicates that the user does not have permission to update the order.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("{orderId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int orderId, OrderUpdateDto updatedOrder, CancellationToken cancellationToken)
    {
        OperationResult<OrderDto> result = await orderService.UpdateOrderAsync(orderId, updatedOrder, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }
}
