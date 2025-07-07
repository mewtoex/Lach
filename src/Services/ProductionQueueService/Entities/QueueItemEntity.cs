using Lach.Shared.Common.Models;

namespace ProductionQueueService.Entities;

public enum QueueItemStatus
{
    Queued,
    InProgress,
    Completed,
    Cancelled
}

public class QueueItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Position { get; set; }
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Queued;
    public string Items { get; set; } = string.Empty; // JSON serialized order items
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
} 