using OrderService.Data;
using OrderService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IMessageBus _messageBus;

    public OrderService(OrderDbContext context, IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, string customerName, string customerPhone, CreateOrderRequest request)
    {
        // Calculate subtotal (delivery fee will be calculated by delivery service)
        var subtotal = request.Items.Sum(item => item.Quantity * 0); // We'll need to get product prices from ProductService
        
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            Subtotal = subtotal,
            DeliveryFee = 0, // Will be calculated by delivery service
            Total = subtotal,
            Status = OrderStatus.Pending,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryInstructions = request.DeliveryInstructions,
            CreatedAt = DateTime.UtcNow
        };

        // Add order items
        foreach (var itemRequest in request.Items)
        {
            var orderItem = new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = itemRequest.ProductId,
                ProductName = "", // Will be populated from ProductService
                UnitPrice = 0, // Will be populated from ProductService
                Quantity = itemRequest.Quantity,
                TotalPrice = 0, // Will be calculated
                SpecialInstructions = itemRequest.SpecialInstructions
            };
            
            order.Items.Add(orderItem);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Publish order created event
        var orderCreatedEvent = new OrderCreatedEvent
        {
            Order = MapToOrder(order)
        };
        
        await _messageBus.PublishAsync(orderCreatedEvent, "order.created");

        return MapToOrder(order);
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order != null ? MapToOrder(order) : null;
    }

    public async Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrder).ToList();
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrder).ToList();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrder).ToList();
    }

    public async Task<Order> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        var previousStatus = order.Status;
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // Update status-specific timestamps
        switch (request.Status)
        {
            case OrderStatus.Accepted:
                order.AcceptedAt = DateTime.UtcNow;
                break;
            case OrderStatus.InProduction:
                order.InProductionAt = DateTime.UtcNow;
                break;
            case OrderStatus.ReadyForDelivery:
                order.ReadyForDeliveryAt = DateTime.UtcNow;
                break;
            case OrderStatus.OutForDelivery:
                order.OutForDeliveryAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync();

        // Publish status updated event
        var statusUpdatedEvent = new OrderStatusUpdatedEvent
        {
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = order.Status,
            Notes = request.Notes
        };

        await _messageBus.PublishAsync(statusUpdatedEvent, "order.status.updated");

        return MapToOrder(order);
    }

    public async Task<bool> CancelOrderAsync(Guid id, string reason)
    {
        var order = await _context.Orders.FindAsync(id);
        
        if (order == null)
        {
            return false;
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish order cancelled event
        var orderCancelledEvent = new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerPhone = order.CustomerPhone,
            Reason = reason
        };

        await _messageBus.PublishAsync(orderCancelledEvent, "order.cancelled");

        return true;
    }

    public async Task<Order> AcceptOrderAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        order.Status = OrderStatus.Accepted;
        order.AcceptedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish order accepted event
        var orderAcceptedEvent = new OrderAcceptedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerPhone = order.CustomerPhone
        };

        await _messageBus.PublishAsync(orderAcceptedEvent, "order.accepted");

        return MapToOrder(order);
    }

    public async Task<Order> RejectOrderAsync(Guid id, string reason)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Publish order cancelled event
        var orderCancelledEvent = new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerPhone = order.CustomerPhone,
            Reason = reason
        };

        await _messageBus.PublishAsync(orderCancelledEvent, "order.rejected");

        return MapToOrder(order);
    }

    private static Order MapToOrder(OrderEntity entity)
    {
        return new Order
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            Items = entity.Items.Select(MapToOrderItem).ToList(),
            Subtotal = entity.Subtotal,
            DeliveryFee = entity.DeliveryFee,
            Total = entity.Total,
            Status = entity.Status,
            DeliveryAddress = entity.DeliveryAddress,
            DeliveryInstructions = entity.DeliveryInstructions,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AcceptedAt = entity.AcceptedAt,
            InProductionAt = entity.InProductionAt,
            ReadyForDeliveryAt = entity.ReadyForDeliveryAt,
            OutForDeliveryAt = entity.OutForDeliveryAt,
            DeliveredAt = entity.DeliveredAt,
            CancelledAt = entity.CancelledAt
        };
    }

    private static OrderItem MapToOrderItem(OrderItemEntity entity)
    {
        return new OrderItem
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            UnitPrice = entity.UnitPrice,
            Quantity = entity.Quantity,
            TotalPrice = entity.TotalPrice,
            SpecialInstructions = entity.SpecialInstructions
        };
    }
} 