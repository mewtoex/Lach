using NotificationService.Data;
using NotificationService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IMessageBus _messageBus;

    public NotificationService(
        NotificationDbContext context, 
        IEmailService emailService, 
        IPushNotificationService pushNotificationService,
        IMessageBus messageBus)
    {
        _context = context;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _messageBus = messageBus;
    }

    public async Task<Notification> SendNotificationAsync(NotificationRequest request)
    {
        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Recipient = request.Recipient,
            Channel = request.Channel,
            Status = NotificationStatus.Pending,
            Metadata = request.Parameters != null ? JsonConvert.SerializeObject(request.Parameters) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send notification based on channel
        var success = await SendNotificationByChannel(notification);

        if (success)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
        }

        await _context.SaveChangesAsync();

        // Publish notification sent event
        var notificationSentEvent = new NotificationSentEvent
        {
            NotificationId = notification.Id,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Channel = notification.Channel,
            Success = success
        };

        await _messageBus.PublishAsync(notificationSentEvent, "notification.sent");

        return MapToNotification(notification);
    }

    public async Task<Notification> SendOrderStatusNotificationAsync(Guid orderId, OrderStatus status, string recipient)
    {
        var (title, message) = GetOrderStatusNotificationContent(orderId, status);

        var request = new NotificationRequest
        {
            Type = NotificationType.OrderStatusChanged,
            Title = title,
            Message = message,
            Recipient = recipient,
            Channel = NotificationChannel.Email,
            Parameters = new Dictionary<string, object>
            {
                { "orderId", orderId },
                { "status", status.ToString() }
            }
        };

        return await SendNotificationAsync(request);
    }

    public async Task<Notification> SendOrderCreatedNotificationAsync(Order order)
    {
        var title = "Pedido Confirmado!";
        var message = $"Seu pedido #{order.Id} foi criado com sucesso. Total: R$ {order.TotalAmount:F2}";

        var request = new NotificationRequest
        {
            Type = NotificationType.OrderCreated,
            Title = title,
            Message = message,
            Recipient = order.CustomerEmail,
            Channel = NotificationChannel.Email,
            Parameters = new Dictionary<string, object>
            {
                { "orderId", order.Id },
                { "totalAmount", order.TotalAmount }
            }
        };

        return await SendNotificationAsync(request);
    }

    public async Task<Notification> SendPaymentNotificationAsync(Guid orderId, bool isSuccess, string recipient)
    {
        var type = isSuccess ? NotificationType.PaymentReceived : NotificationType.PaymentFailed;
        var title = isSuccess ? "Pagamento Confirmado!" : "Falha no Pagamento";
        var message = isSuccess 
            ? $"O pagamento do pedido #{orderId} foi confirmado com sucesso!"
            : $"Houve uma falha no pagamento do pedido #{orderId}. Tente novamente.";

        var request = new NotificationRequest
        {
            Type = type,
            Title = title,
            Message = message,
            Recipient = recipient,
            Channel = NotificationChannel.Email,
            Parameters = new Dictionary<string, object>
            {
                { "orderId", orderId },
                { "success", isSuccess }
            }
        };

        return await SendNotificationAsync(request);
    }

    public async Task<List<Notification>> GetNotificationsByRecipientAsync(string recipient)
    {
        var notifications = await _context.Notifications
            .Where(n => n.Recipient == recipient)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(MapToNotification).ToList();
    }

    public async Task<Notification> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
        }

        notification.Status = NotificationStatus.Read;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToNotification(notification);
    }

    public async Task<Notification> MarkAsDeliveredAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
        }

        notification.Status = NotificationStatus.Delivered;

        await _context.SaveChangesAsync();

        return MapToNotification(notification);
    }

    public async Task<List<Notification>> GetPendingNotificationsAsync()
    {
        var notifications = await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToNotification).ToList();
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null)
        {
            return false;
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<bool> SendNotificationByChannel(NotificationEntity notification)
    {
        return notification.Channel switch
        {
            NotificationChannel.Email => await _emailService.SendEmailAsync(
                notification.Recipient, 
                notification.Title, 
                notification.Message),
            
            NotificationChannel.Push => await _pushNotificationService.SendPushNotificationAsync(
                notification.Recipient, 
                notification.Title, 
                notification.Message),
            
            NotificationChannel.SMS => true, // Placeholder for SMS service
            
            NotificationChannel.WhatsApp => true, // Handled by WhatsApp service
            
            NotificationChannel.InApp => true, // Handled by frontend
            
            _ => false
        };
    }

    private static (string title, string message) GetOrderStatusNotificationContent(Guid orderId, OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Accepted => ("Pedido Aceito!", $"Seu pedido #{orderId} foi aceito e está sendo preparado!"),
            OrderStatus.InProduction => ("Em Preparação!", $"Seu pedido #{orderId} está sendo preparado agora!"),
            OrderStatus.ReadyForDelivery => ("Pronto para Entrega!", $"Seu pedido #{orderId} está pronto para entrega!"),
            OrderStatus.Delivered => ("Pedido Entregue!", $"Seu pedido #{orderId} foi entregue! Obrigado por escolher a Lanchonete Lach!"),
            OrderStatus.Cancelled => ("Pedido Cancelado", $"Seu pedido #{orderId} foi cancelado. Entre em contato conosco se tiver dúvidas."),
            _ => ("Atualização do Pedido", $"Status do pedido #{orderId}: {status}")
        };
    }

    private static Notification MapToNotification(NotificationEntity entity)
    {
        return new Notification
        {
            Id = entity.Id,
            Type = entity.Type,
            Title = entity.Title,
            Message = entity.Message,
            Recipient = entity.Recipient,
            Status = entity.Status,
            Channel = entity.Channel,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt,
            SentAt = entity.SentAt,
            ReadAt = entity.ReadAt
        };
    }
} 