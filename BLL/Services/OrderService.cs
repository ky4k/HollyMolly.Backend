using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Orders;
using HM.DAL.Constants;
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
    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string? userId, IEnumerable<string>? statuses,
        DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken cancellationToken)
    {
        IQueryable<Order> orders = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
                .ThenInclude(or => or.ProductInstance)
                    .ThenInclude(pi => pi.Images)
            .Include(o => o.StatusHistory)
            .Include(o => o.NewPostInternetDocument)
            .AsNoTracking();
        if (userId != null)
        {
            orders = orders.Where(o => o.UserId == userId);
        }
        if (statuses != null && statuses.Any())
        {
            orders = orders.Where(o => statuses.Contains(o.Status));
        }
        if (fromDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate > fromDate);
        }
        if (toDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate < toDate);
        }
        return await orders.Select(o => o.ToOrderDto()).ToListAsync(cancellationToken);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
                .ThenInclude(or => or.ProductInstance)
                    .ThenInclude(pi => pi.Images)
            .Include(o => o.StatusHistory)
            .Include(o => o.NewPostInternetDocument)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return order?.ToOrderDto();
    }

    public async Task<OperationResult<OrderDto>> CreateOrderAsync(OrderCreateDto orderDto, string userId,
        CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return new OperationResult<OrderDto>(false, "User does not exist");
        }
        Order order = new()
        {
            UserId = userId,
            Customer = orderDto.Customer.ToCustomerInfo(user.Email!),
            Status = OrderStatuses.Created,
            StatusHistory = [
                new OrderStatusHistory()
                {
                    Status = OrderStatuses.Created,
                    Date = DateTimeOffset.UtcNow
                }
            ],
            OrderDate = DateTimeOffset.UtcNow,
            PaymentReceived = false,
            OrderRecords = []
        };

        decimal totalCost = 0;
        foreach (OrderRecordCreateDto orderRecordDto in orderDto.OrderRecords)
        {
            OrderRecord? orderRecord = await PrepareOrderRecordAsync(orderRecordDto, cancellationToken);
            if (orderRecord == null)
            {
                continue;
            }
            totalCost += orderRecord.Price * orderRecord.Quantity - orderRecord.Discount;
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
            logger.LogError(ex, "An error occurred while creating order {@Order}", order);
            return new OperationResult<OrderDto>(false, "The order has not been created.");
        }
    }
    private async Task<OrderRecord?> PrepareOrderRecordAsync(OrderRecordCreateDto orderRecordDto, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
                .Include(p => p.ProductInstances)
                    .ThenInclude(pi => pi.Images)
                .FirstOrDefaultAsync(p =>
                    p.ProductInstances.Select(pi => pi.Id).Contains(orderRecordDto.ProductInstanceId),
                    cancellationToken);
        ProductInstance? productInstance = product?.ProductInstances
            .Find(pi => pi.Id == orderRecordDto.ProductInstanceId);
        if (productInstance == null)
        {
            return null;
        }
        if (productInstance.StockQuantity == 0)
        {
            return null;
        }
        if (productInstance.StockQuantity < orderRecordDto.Quantity)
        {
            orderRecordDto.Quantity = productInstance.StockQuantity;
        }
        productInstance.StockQuantity -= orderRecordDto.Quantity;
        return new OrderRecord()
        {
            ProductInstanceId = productInstance.Id,
            ProductName = product!.Name,
            Price = productInstance.Price,
            Quantity = orderRecordDto.Quantity,
            Discount = orderRecordDto.Quantity * productInstance.GetCombinedDiscount()
        };
    }

    public async Task<OperationResult<OrderDto>> UpdateOrderAsync(int orderId, OrderUpdateDto updateDto,
        CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRecords)
                .ThenInclude(or => or.ProductInstance)
                    .ThenInclude(pi => pi.Images)
            .Include(o => o.StatusHistory)
            .Include(o => o.NewPostInternetDocument)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            return new OperationResult<OrderDto>(false, "Order with such an id does not exist.");
        }
        await RestoreProductQuantityAsync(order, updateDto.Status, cancellationToken);
        if (order.Status != updateDto.Status ||
            (!string.IsNullOrEmpty(updateDto.Notes) && order.Notes != updateDto.Notes))
        {
            order.StatusHistory.Add(new OrderStatusHistory()
            {
                Status = updateDto.Status,
                Date = DateTimeOffset.UtcNow,
                Notes = updateDto.Notes ?? string.Empty
            });
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
            logger.LogError(ex, "An error occurred while updating order {@Order}", order);
            return new OperationResult<OrderDto>(false, "The order has not been updated.");
        }
    }

    private async Task RestoreProductQuantityAsync(Order order, string newStatus, CancellationToken cancellationToken)
    {
        if (order.Status == newStatus ||
            (order.Status != OrderStatuses.Cancelled && newStatus != OrderStatuses.Cancelled))
        {
            return;
        }
        foreach (OrderRecord orderRecord in order.OrderRecords)
        {
            ProductInstance? productInstance = await context.Products
                .Include(p => p.ProductInstances)
                .SelectMany(p => p.ProductInstances)
                .FirstOrDefaultAsync(pi => pi.Id == orderRecord.ProductInstanceId, cancellationToken);
            if (newStatus == OrderStatuses.Cancelled)
            {
                productInstance!.StockQuantity += orderRecord.Quantity;
            }
            else
            {
                if (productInstance!.StockQuantity < orderRecord.Quantity)
                {
                    orderRecord.Quantity = productInstance.StockQuantity;
                }
                productInstance.StockQuantity -= orderRecord.Quantity;
            }
        }
    }
}
