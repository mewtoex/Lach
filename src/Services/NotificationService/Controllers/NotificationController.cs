using NotificationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<Notification>> SendNotification([FromBody] NotificationRequest request)
    {
        try
        {
            var notification = await _notificationService.SendNotificationAsync(request);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending notification" });
        }
    }

    [HttpPost("order-status")]
    public async Task<ActionResult<Notification>> SendOrderStatusNotification([FromBody] SendOrderStatusNotificationRequest request)
    {
        try
        {
            var notification = await _notificationService.SendOrderStatusNotificationAsync(request.OrderId, request.Status, request.Recipient);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending order status notification" });
        }
    }

    [HttpPost("order-created")]
    public async Task<ActionResult<Notification>> SendOrderCreatedNotification([FromBody] Order order)
    {
        try
        {
            var notification = await _notificationService.SendOrderCreatedNotificationAsync(order);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending order created notification" });
        }
    }

    [HttpPost("payment")]
    public async Task<ActionResult<Notification>> SendPaymentNotification([FromBody] SendPaymentNotificationRequest request)
    {
        try
        {
            var notification = await _notificationService.SendPaymentNotificationAsync(request.OrderId, request.IsSuccess, request.Recipient);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending payment notification" });
        }
    }

    [HttpGet("recipient/{recipient}")]
    public async Task<ActionResult<List<Notification>>> GetNotificationsByRecipient(string recipient)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByRecipientAsync(recipient);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting notifications" });
        }
    }

    [HttpPut("{notificationId}/read")]
    public async Task<ActionResult<Notification>> MarkAsRead(Guid notificationId)
    {
        try
        {
            var notification = await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(notification);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking notification as read" });
        }
    }

    [HttpPut("{notificationId}/delivered")]
    public async Task<ActionResult<Notification>> MarkAsDelivered(Guid notificationId)
    {
        try
        {
            var notification = await _notificationService.MarkAsDeliveredAsync(notificationId);
            return Ok(notification);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking notification as delivered" });
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<Notification>>> GetPendingNotifications()
    {
        try
        {
            var notifications = await _notificationService.GetPendingNotificationsAsync();
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting pending notifications" });
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<ActionResult> DeleteNotification(Guid notificationId)
    {
        try
        {
            var success = await _notificationService.DeleteNotificationAsync(notificationId);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting notification" });
        }
    }
}

public class SendOrderStatusNotificationRequest
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string Recipient { get; set; } = string.Empty;
}

public class SendPaymentNotificationRequest
{
    public Guid OrderId { get; set; }
    public bool IsSuccess { get; set; }
    public string Recipient { get; set; } = string.Empty;
} 