using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductService.Controllers;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Controllers;

public class CustomerControllerTests : TestBase
{
    private readonly CustomerController _controller;
    private readonly ProductDbContext _dbContext;

    public CustomerControllerTests()
    {
        _dbContext = ServiceProvider.GetRequiredService<ProductDbContext>();
        _controller = new CustomerController(_dbContext);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00"
        };

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        var customer = createdResult!.Value as CustomerDto;
        customer.Should().NotBeNull();
        customer!.Name.Should().Be("João Silva");
        customer.Email.Should().Be("joao@email.com");
        customer.Phone.Should().Be("(11) 99999-9999");
        customer.Cpf.Should().Be("123.456.789-00");
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999"
        };

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "email-invalido",
            Phone = "(11) 99999-9999"
        };

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetCustomer(customer.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var customerDto = okResult!.Value as CustomerDto;
        customerDto.Should().NotBeNull();
        customerDto!.Id.Should().Be(customer.Id);
        customerDto.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetCustomer(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var updateDto = new UpdateCustomerDto
        {
            Name = "João Silva Atualizado",
            Phone = "(11) 88888-8888"
        };

        // Act
        var result = await _controller.UpdateCustomer(customer.Id, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var customerDto = okResult!.Value as CustomerDto;
        customerDto.Should().NotBeNull();
        customerDto!.Name.Should().Be("João Silva Atualizado");
        customerDto.Phone.Should().Be("(11) 88888-8888");
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto
        {
            Name = "Teste"
        };

        // Act
        var result = await _controller.UpdateCustomer(invalidId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteCustomer(customer.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteCustomer(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateAddress_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var addressDto = new CreateCustomerAddressDto
        {
            CustomerId = customer.Id,
            Street = "Rua das Flores",
            Number = "123",
            Complement = "Apto 45",
            Neighborhood = "Centro",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsDefault = true
        };

        // Act
        var result = await _controller.CreateAddress(addressDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        var address = createdResult!.Value as CustomerAddressDto;
        address.Should().NotBeNull();
        address!.Street.Should().Be("Rua das Flores");
        address.Number.Should().Be("123");
        address.City.Should().Be("São Paulo");
        address.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAddress_WithInvalidCustomerId_ShouldReturnBadRequest()
    {
        // Arrange
        var addressDto = new CreateCustomerAddressDto
        {
            CustomerId = Guid.NewGuid(), // Non-existent customer
            Street = "Rua das Flores",
            Number = "123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567"
        };

        // Act
        var result = await _controller.CreateAddress(addressDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAddresses_WithValidCustomerId_ShouldReturnOk()
    {
        // Arrange
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@email.com",
            Phone = "(11) 99999-9999",
            Cpf = "123.456.789-00",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Customers.Add(customer);

        var address = new CustomerAddressEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Street = "Rua das Flores",
            Number = "123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.CustomerAddresses.Add(address);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetAddresses(customer.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addresses = okResult!.Value as List<CustomerAddressDto>;
        addresses.Should().NotBeNull();
        addresses!.Should().HaveCount(1);
        addresses.First().Street.Should().Be("Rua das Flores");
    }

    [Fact]
    public async Task GetAddresses_WithInvalidCustomerId_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetAddresses(invalidId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addresses = okResult!.Value as List<CustomerAddressDto>;
        addresses.Should().NotBeNull();
        addresses!.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAddress_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var address = new CustomerAddressEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Street = "Rua das Flores",
            Number = "123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.CustomerAddresses.Add(address);
        await _dbContext.SaveChangesAsync();

        var updateDto = new UpdateCustomerAddressDto
        {
            Street = "Rua Atualizada",
            Number = "456",
            Complement = "Casa"
        };

        // Act
        var result = await _controller.UpdateAddress(address.Id, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addressDto = okResult!.Value as CustomerAddressDto;
        addressDto.Should().NotBeNull();
        addressDto!.Street.Should().Be("Rua Atualizada");
        addressDto.Number.Should().Be("456");
        addressDto.Complement.Should().Be("Casa");
    }

    [Fact]
    public async Task UpdateAddress_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCustomerAddressDto
        {
            Street = "Rua Teste"
        };

        // Act
        var result = await _controller.UpdateAddress(invalidId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteAddress_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var address = new CustomerAddressEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Street = "Rua das Flores",
            Number = "123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.CustomerAddresses.Add(address);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteAddress(address.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAddress_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteAddress(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
} 