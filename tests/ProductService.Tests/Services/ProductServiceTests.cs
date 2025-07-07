using FluentAssertions;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Services;

public class ProductServiceTests : TestBase
{
    private readonly IProductService _service;

    public ProductServiceTests()
    {
        _service = ServiceProvider.GetRequiredService<IProductService>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Coca-Cola");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_WithValidCategoryId_ShouldReturnProducts()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await DbContext.ProductCategories.ToListAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _service.GetByCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().OnlyContain(p => p.CategoryId == categoryId);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithInvalidCategoryId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetByCategoryAsync(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await DbContext.ProductCategories.ToListAsync();
        var categoryId = categories.First().Id;

        var createDto = new CreateProductDto
        {
            Name = "Novo Produto",
            Description = "Descrição do novo produto",
            Price = 12.50m,
            CategoryId = categoryId,
            ImageUrl = "https://example.com/novo-produto.jpg",
            IsActive = true
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Novo Produto");
        result.Description.Should().Be("Descrição do novo produto");
        result.Price.Should().Be(12.50m);
        result.CategoryId.Should().Be(categoryId);
        result.ImageUrl.Should().Be("https://example.com/novo-produto.jpg");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "",
            Description = "Teste",
            Price = 10.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithNegativePrice_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Teste",
            Price = -10.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        var updateDto = new UpdateProductDto
        {
            Name = "Coca-Cola Atualizada",
            Description = "Descrição atualizada",
            Price = 6.00m
        };

        // Act
        var result = await _service.UpdateAsync(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Coca-Cola Atualizada");
        result.Description.Should().Be("Descrição atualizada");
        result.Price.Should().Be(6.00m);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateProductDto
        {
            Name = "Teste"
        };

        // Act
        var result = await _service.UpdateAsync(invalidId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _service.DeleteAsync(productId);

        // Assert
        result.Should().BeTrue();

        // Verify it's deleted
        var deletedProduct = await _service.GetByIdAsync(productId);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.DeleteAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ShouldReturnMatchingProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _service.SearchAsync("Coca");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().OnlyContain(p => p.Name.Contains("Coca", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnAllProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _service.SearchAsync("");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_WithNonMatchingQuery_ShouldReturnEmptyList()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _service.SearchAsync("ProdutoInexistente");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAddOnsAsync_WithValidProductId_ShouldReturnAddOns()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _service.GetAddOnsAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // One add-on category linked
        result.Should().OnlyContain(c => c.ProductId == productId);
    }

    [Fact]
    public async Task GetAddOnsAsync_WithInvalidProductId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetAddOnsAsync(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
} 