namespace DeliveryService.Entities;

public class DeliveryConfigEntity
{
    public Guid Id { get; set; }
    public decimal BaseFee { get; set; }
    public decimal FeePerKm { get; set; }
    public double MaxDistanceKm { get; set; }
    public decimal FreeDeliveryThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 