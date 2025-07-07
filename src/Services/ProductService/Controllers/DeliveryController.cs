using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryCalculationService _deliveryService;

    public DeliveryController(IDeliveryCalculationService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Calcula taxa de entrega para um endereço do cliente
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<DeliveryCalculationDto>> CalculateDelivery(
        [FromBody] DeliveryCalculationRequest request)
    {
        try
        {
            var calculation = await _deliveryService.CalculateDeliveryAsync(
                request.CustomerAddressId, 
                request.OrderTotal);
            
            return Ok(calculation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Calcula taxa de entrega por coordenadas
    /// </summary>
    [HttpPost("calculate/coordinates")]
    public async Task<ActionResult<DeliveryCalculationDto>> CalculateDeliveryByCoordinates(
        [FromBody] DeliveryCalculationByCoordinatesRequest request)
    {
        try
        {
            var calculation = await _deliveryService.CalculateDeliveryByCoordinatesAsync(
                request.Latitude, 
                request.Longitude, 
                request.OrderTotal);
            
            return Ok(calculation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Verifica se a entrega está disponível para um endereço
    /// </summary>
    [HttpGet("available/{customerAddressId:guid}")]
    public async Task<ActionResult<bool>> IsDeliveryAvailable(Guid customerAddressId)
    {
        var isAvailable = await _deliveryService.IsDeliveryAvailableAsync(customerAddressId);
        return Ok(isAvailable);
    }

    /// <summary>
    /// Verifica se a entrega está disponível por coordenadas
    /// </summary>
    [HttpGet("available/coordinates")]
    public async Task<ActionResult<bool>> IsDeliveryAvailableByCoordinates(
        [FromQuery] decimal latitude, 
        [FromQuery] decimal longitude)
    {
        var isAvailable = await _deliveryService.IsDeliveryAvailableByCoordinatesAsync(latitude, longitude);
        return Ok(isAvailable);
    }

    /// <summary>
    /// Obtém informações da lanchonete
    /// </summary>
    [HttpGet("store")]
    public async Task<ActionResult<StoreInfoDto>> GetStoreInfo()
    {
        try
        {
            var storeInfo = await _deliveryService.GetStoreInfoAsync();
            return Ok(storeInfo);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Calcula distância entre dois pontos
    /// </summary>
    [HttpGet("distance")]
    public async Task<ActionResult<decimal>> CalculateDistance(
        [FromQuery] decimal lat1, 
        [FromQuery] decimal lng1, 
        [FromQuery] decimal lat2, 
        [FromQuery] decimal lng2)
    {
        var distance = await _deliveryService.CalculateDistanceAsync(lat1, lng1, lat2, lng2);
        return Ok(distance);
    }
}

public record DeliveryCalculationRequest
{
    public Guid CustomerAddressId { get; init; }
    public decimal OrderTotal { get; init; }
}

public record DeliveryCalculationByCoordinatesRequest
{
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal OrderTotal { get; init; }
} 