namespace WhatsAppService.Entities;

public enum SessionState
{
    New,
    Greeting,
    Menu,
    Ordering,
    Confirming,
    Completed,
    Cancelled
}

public class WhatsAppSessionEntity
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // connecting, connected, disconnected, qr_ready
    public string? QrCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 