using FluentAssertions;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Services;

public class AddOnCategoryServiceTests : TestBase
{
    private readonly IAddOnCategoryService _service;

    public AddOnCategoryServiceTests()
    {
        _service = ServiceProvider.GetRequiredService<IAddOnCategoryService>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveCategories()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Tamanhos");
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
    public async Task CreateAsync_WithValidData_ShouldCreateCategory()
    {
        // Arrange
        var createDto = new CreateAddOnCategoryDto
        {
            Name = "Novo Tamanho",
            Description = "Nova categoria de tamanhos",
            IsActive = true,
            MaxSelections = 2,
            IsRequired = true
        };

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Novo Tamanho");
        result.Description.Should().Be("Nova categoria de tamanhos");
        result.IsActive.Should().BeTrue();
        result.MaxSelections.Should().Be(2);
        result.IsRequired.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateAddOnCategoryDto
        {
            Name = "",
            Description = "Teste"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        var updateDto = new UpdateAddOnCategoryDto
        {
            Name = "Tamanho Atualizado",
            Description = "Descrição atualizada",
            MaxSelections = 3
        };

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Tamanho Atualizado");
        result.Description.Should().Be("Descrição atualizada");
        result.MaxSelections.Should().Be(3);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateAddOnCategoryDto
        {
            Name = "Teste"
        };

        // Act
        var result = await _service.UpdateAsync(invalidId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        result.Should().BeTrue();

        // Verify it's deleted
        var deletedCategory = await _service.GetByIdAsync(categoryId);
        deletedCategory.Should().BeNull();
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
    public async Task GetAddOnsByCategoryAsync_WithValidCategoryId_ShouldReturnAddOns()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _service.GetAddOnsByCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Pequeno e Grande
        result.Should().OnlyContain(a => a.AddOnCategoryId == categoryId);
    }

    [Fact]
    public async Task GetAddOnsByCategoryAsync_WithInvalidCategoryId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetAddOnsByCategoryAsync(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WithValidCategoryId_ShouldReturnProducts()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _service.GetProductsByCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1); // Coca-Cola linked to Tamanhos
        result.Should().OnlyContain(p => p.ProductId != Guid.Empty);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WithInvalidCategoryId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetProductsByCategoryAsync(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task LinkToProductAsync_WithValidData_ShouldLinkCategoryToProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var products = await DbContext.Products.ToListAsync();
        
        var categoryId = categories.First().Id;
        var productId = products.First().Id;

        var linkDto = new CreateProductAddOnCategoryDto
        {
            ProductId = productId,
            AddOnCategoryId = categoryId,
            MaxSelections = 2,
            IsRequired = true
        };

        // Act
        var result = await _service.LinkToProductAsync(linkDto);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);
        result.AddOnCategoryId.Should().Be(categoryId);
        result.MaxSelections.Should().Be(2);
        result.IsRequired.Should().BeTrue();
    }

    [Fact]
    public async Task UnlinkFromProductAsync_WithValidData_ShouldUnlinkCategoryFromProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var productAddOnCategories = await DbContext.ProductAddOnCategories.ToListAsync();
        var linkId = productAddOnCategories.First().Id;

        // Act
        var result = await _service.UnlinkFromProductAsync(linkId);

        // Assert
        result.Should().BeTrue();

        // Verify it's unlinked
        var unlinkedLink = await DbContext.ProductAddOnCategories.FindAsync(linkId);
        unlinkedLink.Should().BeNull();
    }

    [Fact]
    public async Task UnlinkFromProductAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.UnlinkFromProductAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }
} 