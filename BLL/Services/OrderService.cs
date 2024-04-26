using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Orders;
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
            .Include(o => o.OrderRecords)
            .AsNoTracking();
        if (userId != null)
        {
            orders = orders.Where(o => o.UserId == userId);
        }
        return await orders.Select(o => o.ToOrderDto()).ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return order?.ToOrderDto();
    }

    public async Task<OperationResult<OrderDto>> CreateOrderAsync(OrderCreateDto orderDto, string userId,
        CancellationToken cancellationToken)
    {
        var order = new Order()
        {
            UserId = userId,
            Customer = orderDto.Customer.ToCustomerInfo(),
            OrderDate = DateTimeOffset.UtcNow,
            Status = "Created",
            PaymentReceived = false,
            OrderRecords = []
        };

        decimal totalCost = 0;
        foreach (OrderRecordCreateDto orderRecordDto in orderDto.OrderRecords)
        {
            Product? product = await context.Products
                .Include(p => p.ProductInstances)
                .FirstOrDefaultAsync(p =>
                    p.ProductInstances.Select(pi => pi.Id).Contains(orderRecordDto.ProductInstanceId),
                    cancellationToken);
            ProductInstance? productInstance = product?.ProductInstances
                .Find(pi => pi.Id == orderRecordDto.ProductInstanceId);
            if (productInstance == null)
            {
                continue;
            }
            if (productInstance.StockQuantity == 0)
            {
                continue;
            }
            if (productInstance.StockQuantity < orderRecordDto.Quantity)
            {
                orderRecordDto.Quantity = productInstance.StockQuantity;
            }
            var orderRecord = new OrderRecord()
            {
                ProductInstanceId = productInstance.Id,
                ProductName = product!.Name,
                Price = productInstance.Price,
                Quantity = orderRecordDto.Quantity,
                Discount = orderRecordDto.Quantity * productInstance.GetCombinedDiscount()
            };
            totalCost += orderRecord.Price * orderRecord.Quantity - orderRecord.Discount;

            productInstance.StockQuantity -= orderRecordDto.Quantity;
            order.OrderRecords.Add(orderRecord);
        }

        if (order.OrderRecords.Count == 0)
        {
            return new OperationResult<OrderDto>(false, "The order contains no products.");
        }
        if (totalCost < 20)
        {
            return new OperationResult<OrderDto>(false, "Sorry, we only accept orders starting at 20 hryvnias." +
                $"Total cost of your order currently is {totalCost} hryvnias.");
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
        order.Notes = updateDto.Notes ?? string.Empty;
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
