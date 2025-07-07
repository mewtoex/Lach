using Lach.Shared.Common.Models;

namespace ProductionQueueService.Services;

public interface IProductionQueueService
{
    Task<List<QueueItem>> GetQueueAsync();
    Task<QueueItem?> GetQueueItemByOrderIdAsync(Guid orderId);
    Task<QueueItem> AddToQueueAsync(Guid orderId, string customerName, List<OrderItem> items);
    Task<QueueItem> UpdateQueueItemStatusAsync(Guid orderId, QueueItemStatus status, string? notes = null);
    Task<QueueItem> MoveQueueItemAsync(Guid orderId, int newPosition);
    Task<bool> RemoveFromQueueAsync(Guid orderId);
    Task<QueueItem> StartProductionAsync(Guid orderId);
    Task<QueueItem> CompleteProductionAsync(Guid orderId);
    Task<int> GetQueuePositionAsync(Guid orderId);
    Task<List<QueueItem>> GetActiveQueueAsync();
}

public class QueueItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Position { get; set; }
    public QueueItemStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
} 