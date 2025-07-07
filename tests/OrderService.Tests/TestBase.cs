using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Data;
using OrderService.Services;

namespace OrderService.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly OrderDbContext DbContext;

    protected TestBase()
    {
        var services = new ServiceCollection();

        // Configure in-memory database
        services.AddDbContext<OrderDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Register services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<OrderDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
    }

    protected async Task SeedTestDataAsync()
    {
        // Add test customers
        var customers = new[]
        {
            new CustomerEntity
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                Email = "joao@email.com",
                Phone = "(11) 99999-9999",
                Cpf = "123.456.789-00",
                CreatedAt = DateTime.UtcNow
            },
            new CustomerEntity
            {
                Id = Guid.NewGuid(),
                Name = "Maria Santos",
                Email = "maria@email.com",
                Phone = "(11) 88888-8888",
                Cpf = "987.654.321-00",
                CreatedAt = DateTime.UtcNow
            }
        };

        DbContext.Customers.AddRange(customers);
        await DbContext.SaveChangesAsync();

        // Add test addresses
        var addresses = new[]
        {
            new CustomerAddressEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                Street = "Rua das Flores",
                Number = "123",
                Complement = "Apto 45",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01234-567",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            },
            new CustomerAddressEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                Street = "Av. Paulista",
                Number = "1000",
                Complement = "Sala 100",
                Neighborhood = "Bela Vista",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310-100",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        DbContext.CustomerAddresses.AddRange(addresses);
        await DbContext.SaveChangesAsync();

        // Add test orders
        var orders = new[]
        {
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                CustomerAddressId = addresses[0].Id,
                Status = OrderStatus.Pending,
                TotalAmount = 25.50m,
                DeliveryFee = 3.00m,
                Subtotal = 22.50m,
                PaymentMethod = PaymentMethod.CreditCard,
                DeliveryMethod = DeliveryMethod.Delivery,
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30),
                Notes = "Sem cebola",
                CreatedAt = DateTime.UtcNow
            },
            new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                CustomerAddressId = addresses[1].Id,
                Status = OrderStatus.Confirmed,
                TotalAmount = 18.00m,
                DeliveryFee = 0.00m,
                Subtotal = 18.00m,
                PaymentMethod = PaymentMethod.Pix,
                DeliveryMethod = DeliveryMethod.Pickup,
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(15),
                Notes = "",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        DbContext.Orders.AddRange(orders);
        await DbContext.SaveChangesAsync();

        // Add test order items
        var orderItems = new[]
        {
            new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orders[0].Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Coca-Cola",
                Quantity = 2,
                UnitPrice = 5.50m,
                TotalPrice = 11.00m,
                Notes = "Sem gelo"
            },
            new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orders[0].Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Coxinha",
                Quantity = 1,
                UnitPrice = 8.00m,
                TotalPrice = 8.00m,
                Notes = ""
            },
            new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orders[1].Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Hambúrguer",
                Quantity = 1,
                UnitPrice = 18.00m,
                TotalPrice = 18.00m,
                Notes = ""
            }
        };

        DbContext.OrderItems.AddRange(orderItems);
        await DbContext.SaveChangesAsync();

        // Add test order item add-ons
        var orderItemAddOns = new[]
        {
            new OrderItemAddOnEntity
            {
                Id = Guid.NewGuid(),
                OrderItemId = orderItems[0].Id,
                AddOnId = Guid.NewGuid(),
                AddOnName = "Grande",
                Quantity = 1,
                UnitPrice = 2.00m,
                TotalPrice = 2.00m
            },
            new OrderItemAddOnEntity
            {
                Id = Guid.NewGuid(),
                OrderItemId = orderItems[1].Id,
                AddOnId = Guid.NewGuid(),
                AddOnName = "Queijo Extra",
                Quantity = 1,
                UnitPrice = 1.50m,
                TotalPrice = 1.50m
            }
        };

        DbContext.OrderItemAddOns.AddRange(orderItemAddOns);
        await DbContext.SaveChangesAsync();
    }
} 