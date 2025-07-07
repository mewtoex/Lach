namespace WhatsAppService.Entities;

public enum MessageType
{
    Text,
    Image,
    Document,
    Location,
    Contact
}

public enum MessageDirection
{
    Inbound,
    Outbound
}

public enum MessageStatus
{
    Sent,
    Delivered,
    Read,
    Failed
}

public class WhatsAppMessageEntity
{
    public int Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty; // text, image, audio, video, document, location
    public string Content { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // in, out
    public string Status { get; set; } = string.Empty; // sent, delivered, read, failed
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? CustomerId { get; set; }
    public string? OrderId { get; set; }
} 