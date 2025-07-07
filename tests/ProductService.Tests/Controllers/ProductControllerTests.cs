using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProductService.Controllers;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Controllers;

public class ProductControllerTests : TestBase
{
    private readonly ProductController _controller;
    private readonly IProductService _service;

    public ProductControllerTests()
    {
        _service = ServiceProvider.GetRequiredService<IProductService>();
        _controller = new ProductController(_service);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOkWithProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var product = okResult!.Value as ProductDto;
        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
        product.Name.Should().Be("Coca-Cola");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByCategory_WithValidCategoryId_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await DbContext.ProductCategories.ToListAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _controller.GetByCategory(categoryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(1);
        products.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetByCategory_WithInvalidCategoryId_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetByCategory(invalidId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
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
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        var product = createdResult!.Value as ProductDto;
        product.Should().NotBeNull();
        product!.Name.Should().Be("Novo Produto");
        product.Description.Should().Be("Descrição do novo produto");
        product.Price.Should().Be(12.50m);
        product.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "",
            Description = "Teste",
            Price = 10.00m
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Teste",
            Price = -10.00m
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithInvalidCategoryId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Teste",
            Price = 10.00m,
            CategoryId = Guid.NewGuid() // Non-existent category
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
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
        var result = await _controller.Update(productId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var product = okResult!.Value as ProductDto;
        product.Should().NotBeNull();
        product!.Name.Should().Be("Coca-Cola Atualizada");
        product.Description.Should().Be("Descrição atualizada");
        product.Price.Should().Be(6.00m);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateProductDto
        {
            Name = "Teste"
        };

        // Act
        var result = await _controller.Update(invalidId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.Delete(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.Search("Coca");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(1);
        products.First().Name.Should().Contain("Coca", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnOkWithAllProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.Search("");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Search_WithNonMatchingQuery_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.Search("ProdutoInexistente");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductDto>;
        products.Should().NotBeNull();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAddOns_WithValidProductId_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var products = await _service.GetAllAsync();
        var productId = products.First().Id;

        // Act
        var result = await _controller.GetAddOns(productId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addOnCategories = okResult!.Value as List<ProductAddOnCategoryDto>;
        addOnCategories.Should().NotBeNull();
        addOnCategories!.Should().HaveCount(1); // One add-on category linked
        addOnCategories.First().ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task GetAddOns_WithInvalidProductId_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetAddOns(invalidId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addOnCategories = okResult!.Value as List<ProductAddOnCategoryDto>;
        addOnCategories.Should().NotBeNull();
        addOnCategories!.Should().BeEmpty();
    }
} 