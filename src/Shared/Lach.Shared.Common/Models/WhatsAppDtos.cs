namespace Lach.Shared.Common.Models;

#region WhatsApp DTOs

public class CreateSessionRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? QrCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class QrCodeResponse
{
    public string QrCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ConnectionStatusResponse
{
    public string SessionId { get; set; } = string.Empty;
    public bool Connected { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastCheck { get; set; }
}

public class SendMessageRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class SendMessageResponse
{
    public bool Success { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class WhatsAppMessageResponse
{
    public string MessageId { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? CustomerId { get; set; }
    public string? OrderId { get; set; }
}

public class CreateContactRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Guid? CustomerId { get; set; }
}

public class ContactResponse
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime LastInteraction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProcessMessageRequest
{
    public string FromNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class ChatbotResponse
{
    public bool Success { get; set; }
    public string Response { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
}

public class OrderConfirmationRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public CreateOrderRequest Order { get; set; } = new();
    public string? DeliveryInstructions { get; set; }
}

public class WebhookMessageRequest
{
    public string MessageId { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class WebhookStatusRequest
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class WhatsAppHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

#endregion 