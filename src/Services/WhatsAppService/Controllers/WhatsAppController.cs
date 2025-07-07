using WhatsAppService.Services;
using Microsoft.AspNetCore.Mvc;
using WhatsAppService.Entities;
using Lach.Shared.Common.Models;

namespace WhatsAppService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly IWhatsAppWebService _whatsAppWebService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IWhatsAppService whatsAppService,
        IWhatsAppWebService whatsAppWebService,
        ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _whatsAppWebService = whatsAppWebService;
        _logger = logger;
    }

    #region Session Management

    [HttpPost("sessions")]
    public async Task<ActionResult<WhatsAppSessionEntity>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var session = await _whatsAppService.CreateSessionAsync(request.PhoneNumber);
            
            // Initialize WhatsApp Web session
            await _whatsAppWebService.InitializeAsync(session.SessionId);
            
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WhatsApp session");
            return BadRequest(new { error = "Failed to create session" });
        }
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<WhatsAppSessionEntity>> GetSession(string sessionId)
    {
        var session = await _whatsAppService.GetSessionAsync(sessionId);
        if (session == null)
            return NotFound();

        return Ok(session);
    }

    [HttpGet("sessions/phone/{phoneNumber}")]
    public async Task<ActionResult<WhatsAppSessionEntity>> GetSessionByPhone(string phoneNumber)
    {
        var session = await _whatsAppService.GetSessionByPhoneAsync(phoneNumber);
        if (session == null)
            return NotFound();

        return Ok(session);
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<ActionResult> DeleteSession(string sessionId)
    {
        var success = await _whatsAppService.DeleteSessionAsync(sessionId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    #endregion

    #region QR Code and Connection

    [HttpGet("sessions/{sessionId}/qr")]
    public async Task<ActionResult<QrCodeResponse>> GetQrCode(string sessionId)
    {
        var qrCode = await _whatsAppWebService.GetQrCodeAsync(sessionId);
        if (qrCode == null)
            return NotFound(new { error = "QR code not available" });

        return Ok(new QrCodeResponse { QrCode = qrCode });
    }

    [HttpGet("sessions/{sessionId}/status")]
    public async Task<ActionResult<ConnectionStatusResponse>> GetConnectionStatus(string sessionId)
    {
        var isConnected = await _whatsAppWebService.IsConnectedAsync(sessionId);
        var session = await _whatsAppService.GetSessionAsync(sessionId);

        return Ok(new ConnectionStatusResponse
        {
            SessionId = sessionId,
            Connected = isConnected,
            Status = session?.Status ?? "unknown"
        });
    }

    #endregion

    #region Message Management

    [HttpPost("messages/send")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var success = await _whatsAppService.SendMessageAsync(request.ToNumber, request.Content);
            
            if (success)
            {
                return Ok(new SendMessageResponse
                {
                    Success = true,
                    Message = "Message sent successfully"
                });
            }

            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Failed to send message"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message");
            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Error sending message"
            });
        }
    }

    [HttpGet("messages/{messageId}")]
    public async Task<ActionResult<WhatsAppMessageEntity>> GetMessage(string messageId)
    {
        var message = await _whatsAppService.GetMessageAsync(messageId);
        if (message == null)
            return NotFound();

        return Ok(message);
    }

    [HttpGet("messages/phone/{phoneNumber}")]
    public async Task<ActionResult<List<WhatsAppMessageEntity>>> GetMessagesByPhone(string phoneNumber, [FromQuery] int limit = 50)
    {
        var messages = await _whatsAppService.GetMessagesByPhoneAsync(phoneNumber, limit);
        return Ok(messages);
    }

    #endregion

    #region Contact Management

    [HttpGet("contacts")]
    public async Task<ActionResult<List<WhatsAppContactEntity>>> GetAllContacts()
    {
        var contacts = await _whatsAppService.GetAllContactsAsync();
        return Ok(contacts);
    }

    [HttpGet("contacts/{phoneNumber}")]
    public async Task<ActionResult<WhatsAppContactEntity>> GetContact(string phoneNumber)
    {
        var contact = await _whatsAppService.GetContactAsync(phoneNumber);
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    [HttpPost("contacts")]
    public async Task<ActionResult<WhatsAppContactEntity>> CreateOrUpdateContact([FromBody] CreateContactRequest request)
    {
        var contact = await _whatsAppService.CreateOrUpdateContactAsync(
            request.PhoneNumber, 
            request.Name, 
            request.Email, 
            request.CustomerId);

        return Ok(contact);
    }

    #endregion

    #region Chatbot

    [HttpPost("chatbot/process")]
    public async Task<ActionResult<ChatbotResponse>> ProcessMessage([FromBody] ProcessMessageRequest request)
    {
        try
        {
            var response = await _whatsAppService.ProcessIncomingMessageAsync(request.FromNumber, request.Content);
            
            return Ok(new ChatbotResponse
            {
                Success = true,
                Response = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with chatbot");
            return BadRequest(new ChatbotResponse
            {
                Success = false,
                Response = "Error processing message"
            });
        }
    }

    #endregion

    #region Order Integration

    [HttpPost("orders/status")]
    public async Task<ActionResult<SendMessageResponse>> SendOrderStatusUpdate([FromBody] OrderStatusUpdateRequest request)
    {
        try
        {
            var success = await _whatsAppService.SendOrderStatusUpdateAsync(
                request.ToNumber, 
                request.OrderId, 
                request.Status, 
                request.Message);

            return Ok(new SendMessageResponse
            {
                Success = success,
                Message = success ? "Status update sent" : "Failed to send status update"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order status update");
            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Error sending status update"
            });
        }
    }

    [HttpPost("orders/confirmation")]
    public async Task<ActionResult<SendMessageResponse>> SendOrderConfirmation([FromBody] OrderConfirmationRequest request)
    {
        try
        {
            var success = await _whatsAppService.SendOrderConfirmationAsync(request.ToNumber, request.Order);

            return Ok(new SendMessageResponse
            {
                Success = success,
                Message = success ? "Order confirmation sent" : "Failed to send confirmation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order confirmation");
            return BadRequest(new SendMessageResponse
            {
                Success = false,
                Message = "Error sending confirmation"
            });
        }
    }

    #endregion

    #region Webhook

    [HttpPost("webhook/message")]
    public async Task<ActionResult> HandleIncomingMessage([FromBody] WebhookMessageRequest request)
    {
        try
        {
            await _whatsAppService.HandleMessageReceivedAsync(
                request.MessageId,
                request.FromNumber,
                request.ToNumber,
                request.MessageType,
                request.Content);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook message");
            return BadRequest();
        }
    }

    [HttpPost("webhook/status")]
    public async Task<ActionResult> HandleMessageStatusUpdate([FromBody] WebhookStatusRequest request)
    {
        try
        {
            await _whatsAppService.HandleMessageStatusUpdateAsync(request.MessageId, request.Status);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook status update");
            return BadRequest();
        }
    }

    #endregion

    #region Health Check

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "WhatsAppService",
            timestamp = DateTime.UtcNow
        });
    }

    #endregion
}

#region Request/Response Models

public class CreateSessionRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class QrCodeResponse
{
    public string QrCode { get; set; } = string.Empty;
}

public class ConnectionStatusResponse
{
    public string SessionId { get; set; } = string.Empty;
    public bool Connected { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SendMessageRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SendMessageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreateContactRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Guid? CustomerId { get; set; }
}

public class ProcessMessageRequest
{
    public string FromNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatbotResponse
{
    public bool Success { get; set; }
    public string Response { get; set; } = string.Empty;
}

public class OrderStatusUpdateRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class OrderConfirmationRequest
{
    public string ToNumber { get; set; } = string.Empty;
    public CreateOrderRequest Order { get; set; } = new();
}

public class WebhookMessageRequest
{
    public string MessageId { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class WebhookStatusRequest
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

#endregion 