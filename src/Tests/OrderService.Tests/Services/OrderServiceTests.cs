using OrderService.Services;
using OrderService.Data;
using OrderService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace OrderService.Tests.Services;

public class OrderServiceTests
{
    private readonly OrderDbContext _context;
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly OrderService.Services.OrderService _orderService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _mockMessageBus = new Mock<IMessageBus>();
        _orderService = new OrderService.Services.OrderService(_context, _mockMessageBus.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+5511999999999",
            Items = new List<OrderItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "X-Burger",
                    Quantity = 2,
                    Price = 15.90m
                }
            },
            DeliveryAddress = "Rua das Flores, 123",
            DeliveryInstructions = "Entregar no portão",
            TotalAmount = 31.80m
        };

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be(request.CustomerName);
        result.Status.Should().Be(OrderStatus.Pending);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("X-Burger");

        var orderInDb = await _context.Orders.FirstOrDefaultAsync(o => o.Id == result.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+5511999999999",
            Status = OrderStatus.Pending,
            TotalAmount = 31.80m,
            DeliveryAddress = "Rua das Flores, 123",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerName.Should().Be("Test Customer");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _orderService.GetOrderByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ShouldReturnCustomerOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<OrderEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerName = "Test Customer",
                Status = OrderStatus.Pending,
                TotalAmount = 31.80m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CustomerName = "Test Customer",
                Status = OrderStatus.Delivered,
                TotalAmount = 25.90m,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.GetOrdersByCustomerAsync(customerId);

        // Assert
        result.Should().HaveCount(2);
        result.All(o => o.CustomerId == customerId).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Status = OrderStatus.Pending,
            TotalAmount = 31.80m,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Accepted, "Order accepted");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Accepted);

        var orderInDb = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        orderInDb!.Status.Should().Be(OrderStatus.Accepted);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidOrderId_ShouldThrowException()
    {
        // Arrange
        var invalidOrderId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _orderService.UpdateOrderStatusAsync(invalidOrderId, OrderStatus.Accepted, "Order accepted"));
    }

    [Fact]
    public async Task AcceptOrderAsync_ShouldUpdateStatusToAccepted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Status = OrderStatus.Pending,
            TotalAmount = 31.80m,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.AcceptOrderAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Accepted);
    }

    [Fact]
    public async Task RejectOrderAsync_ShouldUpdateStatusToCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Status = OrderStatus.Pending,
            TotalAmount = 31.80m,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.RejectOrderAsync(orderId, "Product unavailable");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldUpdateStatusToCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderEntity
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            Status = OrderStatus.Accepted,
            TotalAmount = 31.80m,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, "Customer requested cancellation");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 