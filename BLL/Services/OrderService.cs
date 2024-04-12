using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class OrderService(
    HmDbContext context,
    ILogger<OrderService> logger
    ) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string? userId, CancellationToken cancellationToken)
    {
        IQueryable<Order> orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords);
        if (userId != null)
        {
            orders = orders.Where(o => o.UserId == userId);
        }
        List<OrderDto> ordersDto = [];
        foreach (Order order in await orders.ToListAsync(cancellationToken))
        {
            ordersDto.Add(order.ToOrderDto());
        }
        return ordersDto;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            return null;
        }
        return order.ToOrderDto();
    }

    public async Task<OperationResult<OrderDto>> CreateOrderAsync(OrderCreateDto orderDto, string userId,
        CancellationToken cancellationToken)
    {
        var order = new Order()
        {
            UserId = userId,
            Customer = orderDto.Customer.ToCustomerInfo(),
            OrderDate = DateTimeOffset.UtcNow,
            Status = "default",
            PaymentReceived = false,
            OrderRecords = []
        };

        foreach (var orderRecordDto in orderDto.OrderRecords)
        {
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == orderRecordDto.ProductId, cancellationToken);
            if (product == null)
            {
                continue;
            }
            if (product.StockQuantity == 0)
            {
                continue;
            }
            if (product.StockQuantity < orderRecordDto.Quantity)
            {
                orderRecordDto.Quantity = product.StockQuantity;
            }
            var orderRecord = new OrderRecord()
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = orderRecordDto.Quantity
            };

            product.StockQuantity -= orderRecordDto.Quantity;
            order.OrderRecords.Add(orderRecord);
        }

        if (order.OrderRecords.Count == 0)
        {
            return new OperationResult<OrderDto>(false, "The order contains no products.");
        }

        try
        {
            await context.Orders.AddAsync(order, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<OrderDto>(true, order.ToOrderDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating order {order}", order);
            return new OperationResult<OrderDto>(false, "The order has not been created.");
        }
    }

    public async Task<OperationResult<OrderDto>> UpdateOrderAsync(int orderId, OrderUpdateDto updateDto,
        CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            return new OperationResult<OrderDto>(false, "Order with such an id does not exist.");
        }
        order.Status = updateDto.Status;
        order.Notes = updateDto.Notes;
        try
        {
            context.Orders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<OrderDto>(true, order.ToOrderDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating order {order}", order);
            return new OperationResult<OrderDto>(false, "The order has not been updated.");
        }
    }
}
