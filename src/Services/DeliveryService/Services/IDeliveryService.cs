using Lach.Shared.Common.Models;

namespace DeliveryService.Services;

public interface IDeliveryService
{
    Task<DeliveryCalculation> CalculateDeliveryFeeAsync(string deliveryAddress, decimal orderSubtotal);
    Task<RestaurantLocation> GetRestaurantLocationAsync();
    Task<RestaurantLocation> UpdateRestaurantLocationAsync(RestaurantLocation location);
    Task<DeliveryConfig> GetDeliveryConfigAsync();
    Task<DeliveryConfig> UpdateDeliveryConfigAsync(DeliveryConfig config);
    Task<bool> IsDeliveryAvailableAsync(string deliveryAddress);
    Task<double> CalculateDistanceAsync(string deliveryAddress);
}

public class DeliveryCalculation
{
    public decimal BaseFee { get; set; }
    public decimal FeePerKm { get; set; }
    public double DistanceInKm { get; set; }
    public decimal DistanceFee { get; set; }
    public decimal TotalFee { get; set; }
    public bool IsFreeDelivery { get; set; }
    public decimal OrderSubtotal { get; set; }
    public decimal FreeDeliveryThreshold { get; set; }
}

public class RestaurantLocation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DeliveryConfig
{
    public Guid Id { get; set; }
    public decimal BaseFee { get; set; }
    public decimal FeePerKm { get; set; }
    public double MaxDistanceKm { get; set; }
    public decimal FreeDeliveryThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 