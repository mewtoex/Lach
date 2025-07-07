using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Controllers;
using ProductService.Models;
using ProductService.Services;
using ProductService.Tests;

namespace ProductService.Tests.Controllers;

public class AddOnCategoryControllerTests : TestBase
{
    private readonly AddOnCategoryController _controller;
    private readonly IAddOnCategoryService _service;

    public AddOnCategoryControllerTests()
    {
        _service = ServiceProvider.GetRequiredService<IAddOnCategoryService>();
        _controller = new AddOnCategoryController(_service);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithCategories()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var categories = okResult!.Value as List<AddOnCategoryDto>;
        categories.Should().NotBeNull();
        categories!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOkWithCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _controller.GetById(categoryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var category = okResult!.Value as AddOnCategoryDto;
        category.Should().NotBeNull();
        category!.Id.Should().Be(categoryId);
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
    public async Task Create_WithValidData_ShouldReturnCreated()
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
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        var category = createdResult!.Value as AddOnCategoryDto;
        category.Should().NotBeNull();
        category!.Name.Should().Be("Novo Tamanho");
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateAddOnCategoryDto
        {
            Name = "", // Invalid empty name
            Description = "Teste"
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
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        var updateDto = new UpdateAddOnCategoryDto
        {
            Name = "Tamanho Atualizado",
            Description = "Descrição atualizada"
        };

        // Act
        var result = await _controller.Update(categoryId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var category = okResult!.Value as AddOnCategoryDto;
        category.Should().NotBeNull();
        category!.Name.Should().Be("Tamanho Atualizado");
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateAddOnCategoryDto
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
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _controller.Delete(categoryId);

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
    public async Task GetAddOns_WithValidCategoryId_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _controller.GetAddOns(categoryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addOns = okResult!.Value as List<ProductAddOnDto>;
        addOns.Should().NotBeNull();
        addOns!.Should().HaveCount(2); // Pequeno e Grande
    }

    [Fact]
    public async Task GetAddOns_WithInvalidCategoryId_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetAddOns(invalidId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var addOns = okResult!.Value as List<ProductAddOnDto>;
        addOns.Should().NotBeNull();
        addOns!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProducts_WithValidCategoryId_ShouldReturnOk()
    {
        // Arrange
        await SeedTestDataAsync();
        var categories = await _service.GetAllAsync();
        var categoryId = categories.First().Id;

        // Act
        var result = await _controller.GetProducts(categoryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductAddOnCategoryDto>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(1); // Coca-Cola linked to Tamanhos
    }

    [Fact]
    public async Task GetProducts_WithInvalidCategoryId_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetProducts(invalidId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var products = okResult!.Value as List<ProductAddOnCategoryDto>;
        products.Should().NotBeNull();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task LinkToProduct_WithValidData_ShouldReturnCreated()
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
        var result = await _controller.LinkToProduct(linkDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        var link = createdResult!.Value as ProductAddOnCategoryDto;
        link.Should().NotBeNull();
        link!.ProductId.Should().Be(productId);
        link.AddOnCategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task UnlinkFromProduct_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        await SeedTestDataAsync();
        var productAddOnCategories = await DbContext.ProductAddOnCategories.ToListAsync();
        var linkId = productAddOnCategories.First().Id;

        // Act
        var result = await _controller.UnlinkFromProduct(linkId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UnlinkFromProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.UnlinkFromProduct(invalidId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
} 