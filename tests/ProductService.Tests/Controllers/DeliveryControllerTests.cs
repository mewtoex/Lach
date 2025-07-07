using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductService.Controllers;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Controllers;

public class DeliveryControllerTests : TestBase
{
    private readonly DeliveryController _controller;
    private readonly IDeliveryCalculationService _service;
    private readonly ProductDbContext _dbContext;

    public DeliveryControllerTests()
    {
        _service = ServiceProvider.GetRequiredService<IDeliveryCalculationService>();
        _dbContext = ServiceProvider.GetRequiredService<ProductDbContext>();
        _controller = new DeliveryController(_service, _dbContext);
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.5505,
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeliveryFeeResponse;
        response.Should().NotBeNull();
        response!.StoreId.Should().Be(store.Id);
        response.DeliveryFee.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithInvalidStoreId_ShouldReturnNotFound()
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
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithInvalidCoordinates_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = 200.0, // Invalid latitude
            CustomerLongitude = -46.6333,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithNegativeOrderTotal_ShouldReturnBadRequest()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = -23.5505,
            CustomerLongitude = -46.6333,
            OrderTotal = -10.00m
        };

        // Act
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetStoreInfo_WithValidStoreId_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();

        // Act
        var result = await _controller.GetStoreInfo(store.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var storeInfo = okResult!.Value as StoreInfoDto;
        storeInfo.Should().NotBeNull();
        storeInfo!.Id.Should().Be(store.Id);
        storeInfo.Name.Should().Be("Snack Bar Teste");
        storeInfo.Address.Should().Be("Rua Teste, 123");
        storeInfo.City.Should().Be("São Paulo");
        storeInfo.State.Should().Be("SP");
    }

    [Fact]
    public async Task GetStoreInfo_WithInvalidStoreId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetStoreInfo(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetStoreInfo_WithInactiveStore_ShouldReturnNotFound()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        store.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetStoreInfo(store.Id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithinFreeDeliveryThreshold_ShouldReturnFreeDelivery()
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
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeliveryFeeResponse;
        response.Should().NotBeNull();
        response!.IsFreeDelivery.Should().BeTrue();
        response.DeliveryFee.Should().Be(0.00m);
    }

    [Fact]
    public async Task CalculateDeliveryFee_OutsideDeliveryRadius_ShouldReturnUnavailable()
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
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeliveryFeeResponse;
        response.Should().BeNull(); // Delivery unavailable
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithExactDeliveryRadius_ShouldReturnValidFee()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        // Customer location at exact delivery radius (5km)
        var customerLat = store.Latitude + (5.0 / 111.0); // Approximate conversion
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = customerLat,
            CustomerLongitude = store.Longitude,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeliveryFeeResponse;
        response.Should().NotBeNull();
        response!.Distance.Should().BeApproximately(5.0, 0.1);
        response.DeliveryFee.Should().Be(10.50m); // Base fee (3.00) + (5km * 1.50)
    }

    [Fact]
    public async Task CalculateDeliveryFee_WithZeroDistance_ShouldReturnBaseFee()
    {
        // Arrange
        await SeedTestDataAsync();
        var store = await _dbContext.Stores.FirstAsync();
        
        var request = new DeliveryFeeRequest
        {
            StoreId = store.Id,
            CustomerLatitude = store.Latitude, // Same location as store
            CustomerLongitude = store.Longitude,
            OrderTotal = 15.00m
        };

        // Act
        var result = await _controller.CalculateDeliveryFee(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as DeliveryFeeResponse;
        response.Should().NotBeNull();
        response!.Distance.Should().BeApproximately(0.0, 0.01);
        response.DeliveryFee.Should().Be(3.00m); // Base fee only
    }
} 