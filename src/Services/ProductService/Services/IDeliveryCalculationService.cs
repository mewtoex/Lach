using ProductService.Models;

namespace ProductService.Services;

public interface IDeliveryCalculationService
{
    Task<DeliveryCalculationDto> CalculateDeliveryAsync(Guid customerAddressId, decimal orderTotal);
    Task<DeliveryCalculationDto> CalculateDeliveryByCoordinatesAsync(decimal customerLat, decimal customerLng, decimal orderTotal);
    Task<decimal> CalculateDistanceAsync(decimal lat1, decimal lng1, decimal lat2, decimal lng2);
    Task<bool> IsDeliveryAvailableAsync(Guid customerAddressId);
    Task<bool> IsDeliveryAvailableByCoordinatesAsync(decimal customerLat, decimal customerLng);
    Task<StoreInfoDto> GetStoreInfoAsync();
} 