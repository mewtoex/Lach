using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

public class DeliveryCalculationService : IDeliveryCalculationService
{
    private readonly ProductDbContext _context;

    public DeliveryCalculationService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryCalculationDto> CalculateDeliveryAsync(Guid customerAddressId, decimal orderTotal)
    {
        var customerAddress = await _context.CustomerAddresses
            .FirstOrDefaultAsync(ca => ca.Id == customerAddressId && ca.IsActive);

        if (customerAddress == null)
            throw new ArgumentException("Endereço do cliente não encontrado");

        return await CalculateDeliveryByCoordinatesAsync(
            customerAddress.Latitude, 
            customerAddress.Longitude, 
            orderTotal);
    }

    public async Task<DeliveryCalculationDto> CalculateDeliveryByCoordinatesAsync(decimal customerLat, decimal customerLng, decimal orderTotal)
    {
        var store = await _context.Stores
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (store == null)
            throw new InvalidOperationException("Lanchonete não configurada");

        var distance = await CalculateDistanceAsync(
            store.Latitude, store.Longitude, 
            customerLat, customerLng);

        var isAvailable = distance <= store.MaxDeliveryDistance;
        
        decimal deliveryFee = 0;
        if (isAvailable)
        {
            // Calcular taxa de entrega
            deliveryFee = store.DeliveryBasePrice + (distance * store.DeliveryPricePerKm);
            
            // Verificar se tem entrega grátis
            if (orderTotal >= store.FreeDeliveryThreshold)
            {
                deliveryFee = 0;
            }
        }

        var estimatedTime = isAvailable ? store.EstimatedDeliveryTimeMinutes : 0;

        return new DeliveryCalculationDto
        {
            IsDeliveryAvailable = isAvailable,
            Distance = distance,
            DeliveryFee = deliveryFee,
            EstimatedDeliveryTimeMinutes = estimatedTime,
            FreeDeliveryThreshold = store.FreeDeliveryThreshold,
            MaxDeliveryDistance = store.MaxDeliveryDistance,
            BasePrice = store.DeliveryBasePrice,
            PricePerKm = store.DeliveryPricePerKm
        };
    }

    public async Task<decimal> CalculateDistanceAsync(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        // Fórmula de Haversine para calcular distância entre coordenadas
        const double earthRadius = 6371; // Raio da Terra em km

        var lat1Rad = (double)lat1 * Math.PI / 180;
        var lat2Rad = (double)lat2 * Math.PI / 180;
        var deltaLat = ((double)lat2 - (double)lat1) * Math.PI / 180;
        var deltaLng = ((double)lng2 - (double)lng1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadius * c;

        return (decimal)Math.Round(distance, 2);
    }

    public async Task<bool> IsDeliveryAvailableAsync(Guid customerAddressId)
    {
        var customerAddress = await _context.CustomerAddresses
            .FirstOrDefaultAsync(ca => ca.Id == customerAddressId && ca.IsActive);

        if (customerAddress == null) return false;

        return await IsDeliveryAvailableByCoordinatesAsync(
            customerAddress.Latitude, 
            customerAddress.Longitude);
    }

    public async Task<bool> IsDeliveryAvailableByCoordinatesAsync(decimal customerLat, decimal customerLng)
    {
        var store = await _context.Stores
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (store == null) return false;

        var distance = await CalculateDistanceAsync(
            store.Latitude, store.Longitude, 
            customerLat, customerLng);

        return distance <= store.MaxDeliveryDistance;
    }

    public async Task<StoreInfoDto> GetStoreInfoAsync()
    {
        var store = await _context.Stores
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (store == null)
            throw new InvalidOperationException("Lanchonete não configurada");

        return new StoreInfoDto
        {
            Id = store.Id,
            Name = store.Name,
            Description = store.Description,
            Cnpj = store.Cnpj,
            Address = store.Address,
            City = store.City,
            State = store.State,
            ZipCode = store.ZipCode,
            Neighborhood = store.Neighborhood,
            Number = store.Number,
            Complement = store.Complement,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            Phone = store.Phone,
            Email = store.Email,
            Website = store.Website,
            LogoUrl = store.LogoUrl,
            CoverImageUrl = store.CoverImageUrl,
            OpeningTime = store.OpeningTime,
            ClosingTime = store.ClosingTime,
            IsOpenOnWeekends = store.IsOpenOnWeekends,
            IsOpenOnHolidays = store.IsOpenOnHolidays,
            DeliveryBasePrice = store.DeliveryBasePrice,
            DeliveryPricePerKm = store.DeliveryPricePerKm,
            FreeDeliveryThreshold = store.FreeDeliveryThreshold,
            MaxDeliveryDistance = store.MaxDeliveryDistance,
            EstimatedDeliveryTimeMinutes = store.EstimatedDeliveryTimeMinutes
        };
    }
} 