using OrderService.Services;
using Lach.Shared.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // TODO: Get customer info from JWT token
            var customerId = Guid.NewGuid(); // Temporary
            var customerName = "Test Customer"; // Temporary
            var customerPhone = "+5511999999999"; // Temporary

            var order = await _orderService.CreateOrderAsync(customerId, customerName, customerPhone, request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the order" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<Order>>> GetOrdersByCustomer(Guid customerId)
    {
        var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
        return Ok(orders);
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<Order>>> GetOrdersByStatus(OrderStatus status)
    {
        var orders = await _orderService.GetOrdersByStatusAsync(status);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<Order>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the order status" });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelOrder(Guid id, [FromBody] string reason)
    {
        var success = await _orderService.CancelOrderAsync(id, reason);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult<Order>> AcceptOrder(Guid id)
    {
        try
        {
            var order = await _orderService.AcceptOrderAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while accepting the order" });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<Order>> RejectOrder(Guid id, [FromBody] string reason)
    {
        try
        {
            var order = await _orderService.RejectOrderAsync(id, reason);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while rejecting the order" });
        }
    }
} 