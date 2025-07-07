using FluentAssertions;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Services;

public class DeliveryCalculationServiceTests : TestBase
{
    private readonly IDeliveryCalculationService _service;
    private readonly ProductDbContext _dbContext;

    public DeliveryCalculationServiceTests()
    {
        _service = ServiceProvider.GetRequiredService<IDeliveryCalculationService>();
        _dbContext = ServiceProvider.GetRequiredService<ProductDbContext>();
    }

    [Fact]
    public async Task CalculateDeliveryFeeAsync_WithinFreeDeliveryThreshold_ShouldReturnZero()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.5505,
            CustomerLongitude = -46.6333,
            OrderTotal = 35.00m // Above free delivery threshold
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryFee.Should().Be(0.00m);
        result.IsFreeDelivery.Should().BeTrue();
        result.Distance.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculateDeliveryFeeAsync_WithinDeliveryRadius_ShouldCalculateCorrectFee()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        // Customer location 2km away
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.5305, // 2km north
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m // Below free delivery threshold
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryFee.Should().Be(6.00m); // Base fee (3.00) + (2km * 1.50)
        result.IsFreeDelivery.Should().BeFalse();
        result.Distance.Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public async Task CalculateDeliveryFeeAsync_OutsideDeliveryRadius_ShouldReturnNull()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        // Customer location 15km away (outside max delivery distance)
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.4005, // 15km north
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateDeliveryFeeAsync_WithInvalidStoreId_ShouldReturnNull()
    {
        // Arrange
        var request = new DeliveryFeeRequest
        {
            StoreId = Guid.NewGuid(),
            CustomerLatitude = -23.5505,
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateDeliveryFeeAsync_WithInactiveStore_ShouldReturnNull()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        store.IsActive = false;
        await _dbContext.SaveChangesAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.5505,
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStoreInfoAsync_WithValidStoreId_ShouldReturnStoreInfo()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();

        // Act
        var result = await _service.GetStoreInfoAsync(store.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(store.Id);
        result.Name.Should().Be("Snack Bar Teste");
        result.Address.Should().Be("Rua Teste, 123");
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
        result.Latitude.Should().Be(-23.5505);
        result.Longitude.Should().Be(-46.6333);
        result.DeliveryRadius.Should().Be(5.0);
        result.BaseDeliveryFee.Should().Be(3.00m);
        result.PricePerKm.Should().Be(1.50m);
        result.FreeDeliveryThreshold.Should().Be(30.00m);
        result.MaxDeliveryDistance.Should().Be(10.0);
    }

    [Fact]
    public async Task GetStoreInfoAsync_WithInvalidStoreId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetStoreInfoAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateDistance_WithValidCoordinates_ShouldReturnCorrectDistance()
    {
        // Arrange
        var lat1 = -23.5505;
        var lon1 = -46.6333;
        var lat2 = -23.5305; // 2km north
        var lon2 = -46.6333;

        // Act
        var distance = _service.CalculateDistance(lat1, lon1, lat2, lon2);

        // Assert
        distance.Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public async Task CalculateDistance_WithSameCoordinates_ShouldReturnZero()
    {
        // Arrange
        var lat = -23.5505;
        var lon = -46.6333;

        // Act
        var distance = _service.CalculateDistance(lat, lon, lat, lon);

        // Assert
        distance.Should().Be(0.0);
    }

    [Fact]
    public async Task CalculateDistance_WithOppositeCoordinates_ShouldReturnCorrectDistance()
    {
        // Arrange
        var lat1 = -23.5505;
        var lon1 = -46.6333;
        var lat2 = -23.5705; // 2km south
        var lon2 = -46.6333;

        // Act
        var distance = _service.CalculateDistance(lat1, lon1, lat2, lon2);

        // Assert
        distance.Should().BeApproximately(2.0, 0.1);
    }

    [Theory]
    [InlineData(0.0, 3.00)] // Base fee only
    [InlineData(1.0, 4.50)] // Base fee + 1km
    [InlineData(2.0, 6.00)] // Base fee + 2km
    [InlineData(5.0, 10.50)] // Base fee + 5km
    public async Task CalculateDeliveryFeeAsync_WithDifferentDistances_ShouldCalculateCorrectFees(double distanceKm, decimal expectedFee)
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        // Calculate coordinates for the given distance
        var customerLat = store.Latitude + (distanceKm / 111.0); // Approximate conversion
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = customerLat,
            CustomerLongitude = store.Longitude,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _service.CalculateDeliveryFeeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.DeliveryFee.Should().BeApproximately(expectedFee, 0.01m);
        result.Distance.Should().BeApproximately(distanceKm, 0.1);
    }
} 