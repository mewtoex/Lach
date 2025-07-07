using Lach.Shared.Common.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid customerId, string customerName, string customerPhone, CreateOrderRequest request);
    Task<Order?> GetOrderByIdAsync(Guid id);
    Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<Order> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<bool> CancelOrderAsync(Guid id, string reason);
    Task<Order> AcceptOrderAsync(Guid id);
    Task<Order> RejectOrderAsync(Guid id, string reason);
} 