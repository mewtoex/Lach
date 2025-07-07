using DeliveryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<DeliveryCalculation>> CalculateDeliveryFee([FromBody] CalculateDeliveryRequest request)
    {
        try
        {
            var calculation = await _deliveryService.CalculateDeliveryFeeAsync(request.DeliveryAddress, request.OrderSubtotal);
            return Ok(calculation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating delivery fee" });
        }
    }

    [HttpGet("available")]
    public async Task<ActionResult<bool>> IsDeliveryAvailable([FromQuery] string address)
    {
        try
        {
            var isAvailable = await _deliveryService.IsDeliveryAvailableAsync(address);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking delivery availability" });
        }
    }

    [HttpGet("distance")]
    public async Task<ActionResult<double>> CalculateDistance([FromQuery] string address)
    {
        try
        {
            var distance = await _deliveryService.CalculateDistanceAsync(address);
            return Ok(distance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating distance" });
        }
    }

    [HttpGet("location")]
    public async Task<ActionResult<RestaurantLocation>> GetRestaurantLocation()
    {
        try
        {
            var location = await _deliveryService.GetRestaurantLocationAsync();
            return Ok(location);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting restaurant location" });
        }
    }

    [HttpPut("location")]
    public async Task<ActionResult<RestaurantLocation>> UpdateRestaurantLocation([FromBody] RestaurantLocation location)
    {
        try
        {
            var updatedLocation = await _deliveryService.UpdateRestaurantLocationAsync(location);
            return Ok(updatedLocation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating restaurant location" });
        }
    }

    [HttpGet("config")]
    public async Task<ActionResult<DeliveryConfig>> GetDeliveryConfig()
    {
        try
        {
            var config = await _deliveryService.GetDeliveryConfigAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting delivery config" });
        }
    }

    [HttpPut("config")]
    public async Task<ActionResult<DeliveryConfig>> UpdateDeliveryConfig([FromBody] DeliveryConfig config)
    {
        try
        {
            var updatedConfig = await _deliveryService.UpdateDeliveryConfigAsync(config);
            return Ok(updatedConfig);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating delivery config" });
        }
    }
}

public class CalculateDeliveryRequest
{
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal OrderSubtotal { get; set; }
} 