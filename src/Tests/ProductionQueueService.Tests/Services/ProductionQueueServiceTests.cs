using ProductionQueueService.Services;
using ProductionQueueService.Data;
using ProductionQueueService.Entities;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace ProductionQueueService.Tests.Services;

public class ProductionQueueServiceTests
{
    private readonly ProductionQueueDbContext _context;
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly ProductionQueueService.Services.ProductionQueueService _queueService;

    public ProductionQueueServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductionQueueDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductionQueueDbContext(options);
        _mockMessageBus = new Mock<IMessageBus>();
        _queueService = new ProductionQueueService.Services.ProductionQueueService(_context, _mockMessageBus.Object);
    }

    [Fact]
    public async Task AddToQueueAsync_WithValidData_ShouldAddToQueue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerName = "Test Customer";
        var items = new List<OrderItem>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                ProductName = "X-Burger",
                Quantity = 2,
                Price = 15.90m
            }
        };

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _queueService.AddToQueueAsync(orderId, customerName, items);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.CustomerName.Should().Be(customerName);
        result.Status.Should().Be(QueueItemStatus.Queued);
        result.Position.Should().Be(1);

        var queueItemInDb = await _context.QueueItems.FirstOrDefaultAsync(q => q.OrderId == orderId);
        queueItemInDb.Should().NotBeNull();
        queueItemInDb!.Status.Should().Be(QueueItemStatus.Queued);
    }

    [Fact]
    public async Task GetQueueAsync_ShouldReturnAllItems()
    {
        // Arrange
        var queueItems = new List<QueueItemEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerName = "Customer 1",
                Position = 1,
                Status = QueueItemStatus.Queued,
                Items = "[]",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerName = "Customer 2",
                Position = 2,
                Status = QueueItemStatus.InProgress,
                Items = "[]",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.QueueItems.AddRangeAsync(queueItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queueService.GetQueueAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Position.Should().Be(1);
        result[1].Position.Should().Be(2);
    }

    [Fact]
    public async Task GetActiveQueueAsync_ShouldReturnOnlyActiveItems()
    {
        // Arrange
        var queueItems = new List<QueueItemEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerName = "Customer 1",
                Position = 1,
                Status = QueueItemStatus.Queued,
                Items = "[]",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerName = "Customer 2",
                Position = 2,
                Status = QueueItemStatus.InProgress,
                Items = "[]",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerName = "Customer 3",
                Position = 3,
                Status = QueueItemStatus.Completed,
                Items = "[]",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.QueueItems.AddRangeAsync(queueItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queueService.GetActiveQueueAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(q => q.Status == QueueItemStatus.Queued || q.Status == QueueItemStatus.InProgress).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateQueueItemStatusAsync_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = "Test Customer",
            Position = 1,
            Status = QueueItemStatus.Queued,
            Items = "[]",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QueueItems.AddAsync(queueItem);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _queueService.UpdateQueueItemStatusAsync(orderId, QueueItemStatus.InProgress, "Production started");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(QueueItemStatus.InProgress);
        result.StartedAt.Should().NotBeNull();

        var queueItemInDb = await _context.QueueItems.FirstOrDefaultAsync(q => q.OrderId == orderId);
        queueItemInDb!.Status.Should().Be(QueueItemStatus.InProgress);
    }

    [Fact]
    public async Task StartProductionAsync_ShouldUpdateStatusToInProgress()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = "Test Customer",
            Position = 1,
            Status = QueueItemStatus.Queued,
            Items = "[]",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QueueItems.AddAsync(queueItem);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _queueService.StartProductionAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(QueueItemStatus.InProgress);
        result.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteProductionAsync_ShouldUpdateStatusToCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = "Test Customer",
            Position = 1,
            Status = QueueItemStatus.InProgress,
            Items = "[]",
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };

        await _context.QueueItems.AddAsync(queueItem);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _queueService.CompleteProductionAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(QueueItemStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveFromQueueAsync_WithValidOrderId_ShouldRemoveItem()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = "Test Customer",
            Position = 1,
            Status = QueueItemStatus.Queued,
            Items = "[]",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QueueItems.AddAsync(queueItem);
        await _context.SaveChangesAsync();

        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _queueService.RemoveFromQueueAsync(orderId);

        // Assert
        result.Should().BeTrue();

        var queueItemInDb = await _context.QueueItems.FirstOrDefaultAsync(q => q.OrderId == orderId);
        queueItemInDb.Should().BeNull();
    }

    [Fact]
    public async Task RemoveFromQueueAsync_WithInvalidOrderId_ShouldReturnFalse()
    {
        // Arrange
        var invalidOrderId = Guid.NewGuid();

        // Act
        var result = await _queueService.RemoveFromQueueAsync(invalidOrderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetQueuePositionAsync_WithValidOrderId_ShouldReturnPosition()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var queueItem = new QueueItemEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerName = "Test Customer",
            Position = 3,
            Status = QueueItemStatus.Queued,
            Items = "[]",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QueueItems.AddAsync(queueItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queueService.GetQueuePositionAsync(orderId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetQueuePositionAsync_WithInvalidOrderId_ShouldReturnMinusOne()
    {
        // Arrange
        var invalidOrderId = Guid.NewGuid();

        // Act
        var result = await _queueService.GetQueuePositionAsync(invalidOrderId);

        // Assert
        result.Should().Be(-1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 