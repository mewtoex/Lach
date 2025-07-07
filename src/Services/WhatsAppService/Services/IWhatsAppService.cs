using WhatsAppService.Entities;
using Lach.Shared.Common.Models;

namespace WhatsAppService.Services;

public interface IWhatsAppService
{
    // Session Management
    Task<WhatsAppSessionEntity> CreateSessionAsync(string phoneNumber);
    Task<WhatsAppSessionEntity?> GetSessionAsync(string sessionId);
    Task<WhatsAppSessionEntity?> GetSessionByPhoneAsync(string phoneNumber);
    Task<bool> UpdateSessionStatusAsync(string sessionId, string status, string? qrCode = null);
    Task<bool> DeleteSessionAsync(string sessionId);
    
    // Message Management
    Task<WhatsAppMessageEntity> SaveIncomingMessageAsync(string messageId, string fromNumber, string toNumber, string messageType, string content);
    Task<WhatsAppMessageEntity> SaveOutgoingMessageAsync(string messageId, string fromNumber, string toNumber, string messageType, string content);
    Task<WhatsAppMessageEntity?> GetMessageAsync(string messageId);
    Task<List<WhatsAppMessageEntity>> GetMessagesByPhoneAsync(string phoneNumber, int limit = 50);
    Task<bool> UpdateMessageStatusAsync(string messageId, string status);
    Task<bool> MarkMessageAsProcessedAsync(string messageId, string? customerId = null, string? orderId = null);
    
    // Contact Management
    Task<WhatsAppContactEntity> CreateOrUpdateContactAsync(string phoneNumber, string? name = null, string? email = null, Guid? customerId = null);
    Task<WhatsAppContactEntity?> GetContactAsync(string phoneNumber);
    Task<List<WhatsAppContactEntity>> GetAllContactsAsync();
    Task<bool> UpdateContactLastInteractionAsync(string phoneNumber);
    
    // Chatbot Functions
    Task<string> ProcessIncomingMessageAsync(string fromNumber, string content);
    Task<bool> SendMessageAsync(string toNumber, string content);
    Task<bool> SendOrderStatusUpdateAsync(string toNumber, string orderId, string status, string? message = null);
    Task<bool> SendOrderConfirmationAsync(string toNumber, CreateOrderRequest order);
    
    // Webhook/Event Handling
    Task HandleMessageReceivedAsync(string messageId, string fromNumber, string toNumber, string messageType, string content);
    Task HandleMessageStatusUpdateAsync(string messageId, string status);
    Task HandleSessionStatusChangeAsync(string sessionId, string status, string? qrCode = null);
}

public class WhatsAppSession
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public SessionState SessionState { get; set; }
    public DateTime LastInteraction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WhatsAppMessage
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageDirection Direction { get; set; }
    public MessageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
} 