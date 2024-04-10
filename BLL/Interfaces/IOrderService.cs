using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersAsync(string? userId, CancellationToken cancellationToken);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken);
    Task<OperationResult<OrderDto>> CreateOrderAsync(OrderCreateDto orderDto, string? userId,
        CancellationToken cancellationToken);
    Task<OperationResult<OrderDto>> UpdateOrderAsync(int orderId, OrderUpdateDto updateDto,
        CancellationToken cancellationToken);
}
