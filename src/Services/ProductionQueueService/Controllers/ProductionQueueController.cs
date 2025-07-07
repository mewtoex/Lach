using ProductionQueueService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProductionQueueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionQueueController : ControllerBase
{
    private readonly IProductionQueueService _queueService;

    public ProductionQueueController(IProductionQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QueueItem>>> GetQueue()
    {
        var queue = await _queueService.GetQueueAsync();
        return Ok(queue);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<QueueItem>>> GetActiveQueue()
    {
        var queue = await _queueService.GetActiveQueueAsync();
        return Ok(queue);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<QueueItem>> GetQueueItem(Guid orderId)
    {
        var queueItem = await _queueService.GetQueueItemByOrderIdAsync(orderId);
        
        if (queueItem == null)
        {
            return NotFound();
        }

        return Ok(queueItem);
    }

    [HttpGet("order/{orderId}/position")]
    public async Task<ActionResult<int>> GetQueuePosition(Guid orderId)
    {
        var position = await _queueService.GetQueuePositionAsync(orderId);
        
        if (position == -1)
        {
            return NotFound();
        }

        return Ok(position);
    }

    [HttpPost("order/{orderId}/add")]
    public async Task<ActionResult<QueueItem>> AddToQueue(Guid orderId, [FromBody] AddToQueueRequest request)
    {
        try
        {
            var queueItem = await _queueService.AddToQueueAsync(orderId, request.CustomerName, request.Items);
            return CreatedAtAction(nameof(GetQueueItem), new { orderId }, queueItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while adding to queue" });
        }
    }

    [HttpPut("order/{orderId}/status")]
    public async Task<ActionResult<QueueItem>> UpdateStatus(Guid orderId, [FromBody] UpdateQueueStatusRequest request)
    {
        try
        {
            var queueItem = await _queueService.UpdateQueueItemStatusAsync(orderId, request.Status, request.Notes);
            return Ok(queueItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating status" });
        }
    }

    [HttpPut("order/{orderId}/position/{newPosition}")]
    public async Task<ActionResult<QueueItem>> MoveQueueItem(Guid orderId, int newPosition)
    {
        try
        {
            var queueItem = await _queueService.MoveQueueItemAsync(orderId, newPosition);
            return Ok(queueItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while moving queue item" });
        }
    }

    [HttpPost("order/{orderId}/start")]
    public async Task<ActionResult<QueueItem>> StartProduction(Guid orderId)
    {
        try
        {
            var queueItem = await _queueService.StartProductionAsync(orderId);
            return Ok(queueItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while starting production" });
        }
    }

    [HttpPost("order/{orderId}/complete")]
    public async Task<ActionResult<QueueItem>> CompleteProduction(Guid orderId)
    {
        try
        {
            var queueItem = await _queueService.CompleteProductionAsync(orderId);
            return Ok(queueItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while completing production" });
        }
    }

    [HttpDelete("order/{orderId}")]
    public async Task<ActionResult> RemoveFromQueue(Guid orderId)
    {
        var success = await _queueService.RemoveFromQueueAsync(orderId);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public class AddToQueueRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

public class UpdateQueueStatusRequest
{
    public QueueItemStatus Status { get; set; }
    public string? Notes { get; set; }
} 