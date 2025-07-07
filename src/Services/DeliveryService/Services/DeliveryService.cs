using DeliveryService.Data;
using DeliveryService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class DeliveryService : IDeliveryService
{
    private readonly DeliveryDbContext _context;
    private readonly IGoogleMapsService _googleMapsService;
    private readonly IMessageBus _messageBus;

    public DeliveryService(DeliveryDbContext context, IGoogleMapsService googleMapsService, IMessageBus messageBus)
    {
        _context = context;
        _googleMapsService = googleMapsService;
        _messageBus = messageBus;
    }

    public async Task<DeliveryCalculation> CalculateDeliveryFeeAsync(string deliveryAddress, decimal orderSubtotal)
    {
        var config = await GetDeliveryConfigAsync();
        var restaurantLocation = await GetRestaurantLocationAsync();
        
        if (restaurantLocation == null)
        {
            throw new InvalidOperationException("Restaurant location not configured");
        }

        // Geocode delivery address
        var geocodingResult = await _googleMapsService.GeocodeAddressAsync(deliveryAddress);
        if (!geocodingResult.IsValid)
        {
            throw new InvalidOperationException("Invalid delivery address");
        }

        // Calculate distance
        var distance = await _googleMapsService.CalculateDistanceAsync(
            restaurantLocation.Latitude, 
            restaurantLocation.Longitude,
            geocodingResult.Latitude, 
            geocodingResult.Longitude);

        // Check if delivery is available
        if (distance > config.MaxDistanceKm)
        {
            throw new InvalidOperationException($"Delivery not available for distances over {config.MaxDistanceKm}km");
        }

        // Calculate fees
        var distanceFee = (decimal)distance * config.FeePerKm;
        var totalFee = config.BaseFee + distanceFee;

        // Check for free delivery
        var isFreeDelivery = orderSubtotal >= config.FreeDeliveryThreshold;
        if (isFreeDelivery)
        {
            totalFee = 0;
        }

        var calculation = new DeliveryCalculation
        {
            BaseFee = config.BaseFee,
            FeePerKm = config.FeePerKm,
            DistanceInKm = distance,
            DistanceFee = distanceFee,
            TotalFee = totalFee,
            IsFreeDelivery = isFreeDelivery,
            OrderSubtotal = orderSubtotal,
            FreeDeliveryThreshold = config.FreeDeliveryThreshold
        };

        // Publish delivery fee calculated event
        var deliveryFeeCalculatedEvent = new DeliveryFeeCalculatedEvent
        {
            OrderId = Guid.NewGuid(), // This should come from the order context
            DeliveryFee = totalFee,
            DistanceInKm = distance
        };

        await _messageBus.PublishAsync(deliveryFeeCalculatedEvent, "delivery.fee.calculated");

        return calculation;
    }

    public async Task<RestaurantLocation> GetRestaurantLocationAsync()
    {
        var location = await _context.RestaurantLocations
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (location == null)
        {
            // Create default location if none exists
            location = new RestaurantLocationEntity
            {
                Id = Guid.NewGuid(),
                Name = "Lanchonete Lach",
                Address = "Rua das Flores, 123 - São Paulo, SP",
                Latitude = -23.5505,
                Longitude = -46.6333,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.RestaurantLocations.Add(location);
            await _context.SaveChangesAsync();
        }

        return MapToRestaurantLocation(location);
    }

    public async Task<RestaurantLocation> UpdateRestaurantLocationAsync(RestaurantLocation location)
    {
        var existingLocation = await _context.RestaurantLocations
            .Where(r => r.IsActive)
            .FirstOrDefaultAsync();

        if (existingLocation != null)
        {
            existingLocation.IsActive = false;
            existingLocation.UpdatedAt = DateTime.UtcNow;
        }

        var newLocation = new RestaurantLocationEntity
        {
            Id = Guid.NewGuid(),
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.RestaurantLocations.Add(newLocation);
        await _context.SaveChangesAsync();

        return MapToRestaurantLocation(newLocation);
    }

    public async Task<DeliveryConfig> GetDeliveryConfigAsync()
    {
        var config = await _context.DeliveryConfigs
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            // Create default config if none exists
            config = new DeliveryConfigEntity
            {
                Id = Guid.NewGuid(),
                BaseFee = 5.00m,
                FeePerKm = 2.00m,
                MaxDistanceKm = 15.0,
                FreeDeliveryThreshold = 50.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        return MapToDeliveryConfig(config);
    }

    public async Task<DeliveryConfig> UpdateDeliveryConfigAsync(DeliveryConfig config)
    {
        var existingConfig = await _context.DeliveryConfigs
            .Where(c => c.IsActive)
            .FirstOrDefaultAsync();

        if (existingConfig != null)
        {
            existingConfig.IsActive = false;
            existingConfig.UpdatedAt = DateTime.UtcNow;
        }

        var newConfig = new DeliveryConfigEntity
        {
            Id = Guid.NewGuid(),
            BaseFee = config.BaseFee,
            FeePerKm = config.FeePerKm,
            MaxDistanceKm = config.MaxDistanceKm,
            FreeDeliveryThreshold = config.FreeDeliveryThreshold,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DeliveryConfigs.Add(newConfig);
        await _context.SaveChangesAsync();

        return MapToDeliveryConfig(newConfig);
    }

    public async Task<bool> IsDeliveryAvailableAsync(string deliveryAddress)
    {
        try
        {
            var config = await GetDeliveryConfigAsync();
            var restaurantLocation = await GetRestaurantLocationAsync();
            
            if (restaurantLocation == null)
            {
                return false;
            }

            var distance = await CalculateDistanceAsync(deliveryAddress);
            return distance <= config.MaxDistanceKm;
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> CalculateDistanceAsync(string deliveryAddress)
    {
        var restaurantLocation = await GetRestaurantLocationAsync();
        
        if (restaurantLocation == null)
        {
            throw new InvalidOperationException("Restaurant location not configured");
        }

        var geocodingResult = await _googleMapsService.GeocodeAddressAsync(deliveryAddress);
        if (!geocodingResult.IsValid)
        {
            throw new InvalidOperationException("Invalid delivery address");
        }

        return await _googleMapsService.CalculateDistanceAsync(
            restaurantLocation.Latitude,
            restaurantLocation.Longitude,
            geocodingResult.Latitude,
            geocodingResult.Longitude);
    }

    private static RestaurantLocation MapToRestaurantLocation(RestaurantLocationEntity entity)
    {
        return new RestaurantLocation
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static DeliveryConfig MapToDeliveryConfig(DeliveryConfigEntity entity)
    {
        return new DeliveryConfig
        {
            Id = entity.Id,
            BaseFee = entity.BaseFee,
            FeePerKm = entity.FeePerKm,
            MaxDistanceKm = entity.MaxDistanceKm,
            FreeDeliveryThreshold = entity.FreeDeliveryThreshold,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
} 