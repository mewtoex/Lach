using ProductionQueueService.Data;
using ProductionQueueService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ProductionQueueService.Services;

public class ProductionQueueService : IProductionQueueService
{
    private readonly ProductionQueueDbContext _context;
    private readonly IMessageBus _messageBus;

    public ProductionQueueService(ProductionQueueDbContext context, IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }

    public async Task<List<QueueItem>> GetQueueAsync()
    {
        var queueItems = await _context.QueueItems
            .OrderBy(q => q.Position)
            .ToListAsync();

        return queueItems.Select(MapToQueueItem).ToList();
    }

    public async Task<QueueItem?> GetQueueItemByOrderIdAsync(Guid orderId)
    {
        var queueItem = await _context.QueueItems
            .FirstOrDefaultAsync(q => q.OrderId == orderId);

        return queueItem != null ? MapToQueueItem(queueItem) : null;
    }

    public async Task<QueueItem> AddToQueueAsync(Guid orderId, string customerName, List<OrderItem> items)
    {
        // Get next position
        var nextPosition = await _context.QueueItems
            .Where(q => q.Status == QueueItemStatus.Queued || q.Status == QueueItemStatus.InProgress)
            .MaxAsync(q => (int?)q.Position) ?? 0;
        nextPosition++;

        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = customerName,
            Position = nextPosition,
            Status = QueueItemStatus.Queued,
            Items = JsonConvert.SerializeObject(items),
            CreatedAt = DateTime.UtcNow
        };

        _context.QueueItems.Add(queueItem);
        await _context.SaveChangesAsync();

        // Publish event
        var orderAddedToQueueEvent = new OrderAddedToQueueEvent
        {
            OrderId = orderId,
            Position = nextPosition
        };

        await _messageBus.PublishAsync(orderAddedToQueueEvent, "production.queue.added");

        return MapToQueueItem(queueItem);
    }

    public async Task<QueueItem> UpdateQueueItemStatusAsync(Guid orderId, QueueItemStatus status, string? notes = null)
    {
        var queueItem = await _context.QueueItems
            .FirstOrDefaultAsync(q => q.OrderId == orderId);

        if (queueItem == null)
        {
            throw new KeyNotFoundException($"Queue item with OrderId {orderId} not found");
        }

        var previousStatus = queueItem.Status;
        queueItem.Status = status;
        queueItem.Notes = notes;

        // Update timestamps based on status
        switch (status)
        {
            case QueueItemStatus.InProgress:
                queueItem.StartedAt = DateTime.UtcNow;
                break;
            case QueueItemStatus.Completed:
                queueItem.CompletedAt = DateTime.UtcNow;
                break;
        }

        await _context.SaveChangesAsync();

        // Publish event if status changed
        if (previousStatus != status)
        {
            var statusUpdatedEvent = new OrderStatusUpdatedEvent
            {
                OrderId = orderId,
                PreviousStatus = MapToOrderStatus(previousStatus),
                NewStatus = MapToOrderStatus(status),
                Notes = notes
            };

            await _messageBus.PublishAsync(statusUpdatedEvent, "production.queue.status.updated");
        }

        return MapToQueueItem(queueItem);
    }

    public async Task<QueueItem> MoveQueueItemAsync(Guid orderId, int newPosition)
    {
        var queueItem = await _context.QueueItems
            .FirstOrDefaultAsync(q => q.OrderId == orderId);

        if (queueItem == null)
        {
            throw new KeyNotFoundException($"Queue item with OrderId {orderId} not found");
        }

        var oldPosition = queueItem.Position;
        queueItem.Position = newPosition;

        await _context.SaveChangesAsync();

        // Publish event
        var queueOrderChangedEvent = new QueueOrderChangedEvent
        {
            OrderId = orderId,
            NewPosition = newPosition
        };

        await _messageBus.PublishAsync(queueOrderChangedEvent, "production.queue.order.changed");

        return MapToQueueItem(queueItem);
    }

    public async Task<bool> RemoveFromQueueAsync(Guid orderId)
    {
        var queueItem = await _context.QueueItems
            .FirstOrDefaultAsync(q => q.OrderId == orderId);

        if (queueItem == null)
        {
            return false;
        }

        _context.QueueItems.Remove(queueItem);
        await _context.SaveChangesAsync();

        // Publish event
        var orderRemovedFromQueueEvent = new OrderRemovedFromQueueEvent
        {
            OrderId = orderId
        };

        await _messageBus.PublishAsync(orderRemovedFromQueueEvent, "production.queue.removed");

        return true;
    }

    public async Task<QueueItem> StartProductionAsync(Guid orderId)
    {
        return await UpdateQueueItemStatusAsync(orderId, QueueItemStatus.InProgress, "Produção iniciada");
    }

    public async Task<QueueItem> CompleteProductionAsync(Guid orderId)
    {
        return await UpdateQueueItemStatusAsync(orderId, QueueItemStatus.Completed, "Produção concluída");
    }

    public async Task<int> GetQueuePositionAsync(Guid orderId)
    {
        var queueItem = await _context.QueueItems
            .FirstOrDefaultAsync(q => q.OrderId == orderId);

        return queueItem?.Position ?? -1;
    }

    public async Task<List<QueueItem>> GetActiveQueueAsync()
    {
        var queueItems = await _context.QueueItems
            .Where(q => q.Status == QueueItemStatus.Queued || q.Status == QueueItemStatus.InProgress)
            .OrderBy(q => q.Position)
            .ToListAsync();

        return queueItems.Select(MapToQueueItem).ToList();
    }

    private static QueueItem MapToQueueItem(QueueItemEntity entity)
    {
        var items = !string.IsNullOrEmpty(entity.Items) 
            ? JsonConvert.DeserializeObject<List<OrderItem>>(entity.Items) ?? new List<OrderItem>()
            : new List<OrderItem>();

        return new QueueItem
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            CustomerName = entity.CustomerName,
            Position = entity.Position,
            Status = entity.Status,
            Items = items,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt
        };
    }

    private static OrderStatus MapToOrderStatus(QueueItemStatus queueStatus)
    {
        return queueStatus switch
        {
            QueueItemStatus.Queued => OrderStatus.Accepted,
            QueueItemStatus.InProgress => OrderStatus.InProduction,
            QueueItemStatus.Completed => OrderStatus.ReadyForDelivery,
            QueueItemStatus.Cancelled => OrderStatus.Cancelled,
            _ => OrderStatus.Pending
        };
    }
} 