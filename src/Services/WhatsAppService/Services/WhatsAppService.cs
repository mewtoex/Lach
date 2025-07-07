using WhatsAppService.Data;
using WhatsAppService.Entities;
using WhatsAppService.Services;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WhatsAppService.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly WhatsAppDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IWhatsAppWebService _whatsAppWebService;

    public WhatsAppService(
        WhatsAppDbContext context,
        IMessageBus messageBus,
        ILogger<WhatsAppService> logger,
        IWhatsAppWebService whatsAppWebService)
    {
        _context = context;
        _messageBus = messageBus;
        _logger = logger;
        _whatsAppWebService = whatsAppWebService;
    }

    #region Session Management

    public async Task<WhatsAppSessionEntity> CreateSessionAsync(string phoneNumber)
    {
        var session = new WhatsAppSessionEntity
        {
            SessionId = Guid.NewGuid().ToString(),
            PhoneNumber = phoneNumber,
            Status = "connecting",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created WhatsApp session for phone: {PhoneNumber}", phoneNumber);
        return session;
    }

    public async Task<WhatsAppSessionEntity?> GetSessionAsync(string sessionId)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<WhatsAppSessionEntity?> GetSessionByPhoneAsync(string phoneNumber)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);
    }

    public async Task<bool> UpdateSessionStatusAsync(string sessionId, string status, string? qrCode = null)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return false;

        session.Status = status;
        session.QrCode = qrCode;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated session {SessionId} status to: {Status}", sessionId, status);
        return true;
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted session: {SessionId}", sessionId);
        return true;
    }

    #endregion

    #region Message Management

    public async Task<WhatsAppMessageEntity> SaveIncomingMessageAsync(string messageId, string fromNumber, string toNumber, string messageType, string content)
    {
        var message = new WhatsAppMessageEntity
        {
            MessageId = messageId,
            FromNumber = fromNumber,
            ToNumber = toNumber,
            MessageType = messageType,
            Content = content,
            Direction = "in",
            Status = "received",
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved incoming message from {FromNumber}: {Content}", fromNumber, content);
        return message;
    }

    public async Task<WhatsAppMessageEntity> SaveOutgoingMessageAsync(string messageId, string fromNumber, string toNumber, string messageType, string content)
    {
        var message = new WhatsAppMessageEntity
        {
            MessageId = messageId,
            FromNumber = fromNumber,
            ToNumber = toNumber,
            MessageType = messageType,
            Content = content,
            Direction = "out",
            Status = "sent",
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved outgoing message to {ToNumber}: {Content}", toNumber, content);
        return message;
    }

    public async Task<WhatsAppMessageEntity?> GetMessageAsync(string messageId)
    {
        return await _context.Messages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);
    }

    public async Task<List<WhatsAppMessageEntity>> GetMessagesByPhoneAsync(string phoneNumber, int limit = 50)
    {
        return await _context.Messages
            .Where(m => m.FromNumber == phoneNumber || m.ToNumber == phoneNumber)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<bool> UpdateMessageStatusAsync(string messageId, string status)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);

        if (message == null)
            return false;

        message.Status = status;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated message {MessageId} status to: {Status}", messageId, status);
        return true;
    }

    public async Task<bool> MarkMessageAsProcessedAsync(string messageId, string? customerId = null, string? orderId = null)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);

        if (message == null)
            return false;

        message.ProcessedAt = DateTime.UtcNow;
        message.CustomerId = customerId;
        message.OrderId = orderId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked message {MessageId} as processed", messageId);
        return true;
    }

    #endregion

    #region Contact Management

    public async Task<WhatsAppContactEntity> CreateOrUpdateContactAsync(string phoneNumber, string? name = null, string? email = null, Guid? customerId = null)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

        if (contact == null)
        {
            contact = new WhatsAppContactEntity
            {
                PhoneNumber = phoneNumber,
                Name = name,
                Email = email,
                CustomerId = customerId,
                LastInteraction = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);
        }
        else
        {
            contact.Name = name ?? contact.Name;
            contact.Email = email ?? contact.Email;
            contact.CustomerId = customerId ?? contact.CustomerId;
            contact.LastInteraction = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<WhatsAppContactEntity?> GetContactAsync(string phoneNumber)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
    }

    public async Task<List<WhatsAppContactEntity>> GetAllContactsAsync()
    {
        return await _context.Contacts
            .OrderByDescending(c => c.LastInteraction)
            .ToListAsync();
    }

    public async Task<bool> UpdateContactLastInteractionAsync(string phoneNumber)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

        if (contact == null)
            return false;

        contact.LastInteraction = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Chatbot Functions

    public async Task<string> ProcessIncomingMessageAsync(string fromNumber, string content)
    {
        var lowerContent = content.ToLower().Trim();

        // Update contact last interaction
        await UpdateContactLastInteractionAsync(fromNumber);

        // Simple chatbot logic
        if (lowerContent.Contains("cardapio") || lowerContent.Contains("menu"))
        {
            return "🍔 *CARDÁPIO LACH*\n\n" +
                   "• X-Burger - R$ 15,90\n" +
                   "• X-Salada - R$ 17,90\n" +
                   "• X-Bacon - R$ 19,90\n" +
                   "• Batata Frita - R$ 8,90\n" +
                   "• Refrigerante - R$ 5,90\n\n" +
                   "Para fazer um pedido, digite: *pedido*";
        }
        else if (lowerContent.Contains("pedido") || lowerContent.Contains("fazer pedido"))
        {
            return "📝 *FAZER PEDIDO*\n\n" +
                   "Para fazer seu pedido, acesse nosso site:\n" +
                   "🌐 http://localhost:3000\n\n" +
                   "Ou me envie sua lista de itens e endereço de entrega!";
        }
        else if (lowerContent.Contains("status") || lowerContent.Contains("pedido"))
        {
            return "📊 *STATUS DO PEDIDO*\n\n" +
                   "Para verificar o status do seu pedido, acesse:\n" +
                   "🌐 http://localhost:3000/orders\n\n" +
                   "Ou me informe o número do seu pedido!";
        }
        else if (lowerContent.Contains("ajuda") || lowerContent.Contains("help"))
        {
            return "🤖 *AJUDA LACH*\n\n" +
                   "Comandos disponíveis:\n" +
                   "• *cardapio* - Ver nosso cardápio\n" +
                   "• *pedido* - Fazer um pedido\n" +
                   "• *status* - Verificar status do pedido\n" +
                   "• *ajuda* - Esta mensagem\n\n" +
                   "Ou fale com um atendente: (11) 99999-9999";
        }
        else
        {
            return "Olá! 👋 Bem-vindo ao *LACH*\n\n" +
                   "Como posso te ajudar hoje?\n\n" +
                   "Digite *ajuda* para ver nossas opções!";
        }
    }

    public async Task<bool> SendMessageAsync(string toNumber, string content)
    {
        try
        {
            var messageId = Guid.NewGuid().ToString();
            var result = await _whatsAppWebService.SendMessageAsync(toNumber, content);
            
            if (result)
            {
                await SaveOutgoingMessageAsync(messageId, "system", toNumber, "text", content);
                _logger.LogInformation("Message sent successfully to {ToNumber}", toNumber);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to {ToNumber}", toNumber);
            return false;
        }
    }

    public async Task<bool> SendOrderStatusUpdateAsync(string toNumber, string orderId, string status, string? message = null)
    {
        var statusText = status switch
        {
            "Pending" => "⏳ Pendente",
            "Accepted" => "✅ Aceito",
            "InProgress" => "👨‍🍳 Em preparo",
            "Ready" => "🚚 Pronto para entrega",
            "Delivered" => "🎉 Entregue",
            "Cancelled" => "❌ Cancelado",
            _ => status
        };

        var content = $"📦 *ATUALIZAÇÃO DO PEDIDO #{orderId}*\n\n" +
                     $"Status: {statusText}\n";

        if (!string.IsNullOrEmpty(message))
        {
            content += $"\nMensagem: {message}";
        }

        content += "\n\nAcompanhe seu pedido em tempo real:\n🌐 http://localhost:3000/orders";

        return await SendMessageAsync(toNumber, content);
    }

    public async Task<bool> SendOrderConfirmationAsync(string toNumber, CreateOrderRequest order)
    {
        var content = $"🎉 *PEDIDO CONFIRMADO!*\n\n" +
                     $"Pedido: #{order.CustomerId}\n" +
                     $"Cliente: {order.CustomerName}\n" +
                     $"Total: R$ {order.TotalAmount:F2}\n" +
                     $"Endereço: {order.DeliveryAddress}\n\n" +
                     $"Itens:\n";

        foreach (var item in order.Items)
        {
            content += $"• {item.Quantity}x {item.ProductName} - R$ {item.Price:F2}\n";
        }

        content += "\n⏰ Tempo estimado: 30-45 minutos\n\n" +
                  "Acompanhe seu pedido:\n🌐 http://localhost:3000/orders";

        return await SendMessageAsync(toNumber, content);
    }

    #endregion

    #region Webhook/Event Handling

    public async Task HandleMessageReceivedAsync(string messageId, string fromNumber, string toNumber, string messageType, string content)
    {
        try
        {
            // Save message to database
            await SaveIncomingMessageAsync(messageId, fromNumber, toNumber, messageType, content);

            // Process with chatbot
            var response = await ProcessIncomingMessageAsync(fromNumber, content);

            // Send response
            await SendMessageAsync(fromNumber, response);

            // Publish event to message bus
            await _messageBus.PublishAsync(new
            {
                EventType = "WhatsAppMessageReceived",
                MessageId = messageId,
                FromNumber = fromNumber,
                Content = content,
                ProcessedAt = DateTime.UtcNow
            }, "whatsapp.events");

            _logger.LogInformation("Processed incoming message from {FromNumber}", fromNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling incoming message from {FromNumber}", fromNumber);
        }
    }

    public async Task HandleMessageStatusUpdateAsync(string messageId, string status)
    {
        try
        {
            await UpdateMessageStatusAsync(messageId, status);

            await _messageBus.PublishAsync(new
            {
                EventType = "WhatsAppMessageStatusUpdated",
                MessageId = messageId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            }, "whatsapp.events");

            _logger.LogInformation("Updated message status: {MessageId} -> {Status}", messageId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating message status: {MessageId}", messageId);
        }
    }

    public async Task HandleSessionStatusChangeAsync(string sessionId, string status, string? qrCode = null)
    {
        try
        {
            await UpdateSessionStatusAsync(sessionId, status, qrCode);

            await _messageBus.PublishAsync(new
            {
                EventType = "WhatsAppSessionStatusChanged",
                SessionId = sessionId,
                Status = status,
                HasQrCode = !string.IsNullOrEmpty(qrCode),
                UpdatedAt = DateTime.UtcNow
            }, "whatsapp.events");

            _logger.LogInformation("Session status changed: {SessionId} -> {Status}", sessionId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session status: {SessionId}", sessionId);
        }
    }

    #endregion
} 