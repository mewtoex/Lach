using ProductService.Data;
using ProductService.Entities;
using Lach.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _context;

    public ProductService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToProduct).ToList();
    }

    public async Task<List<Product>> GetAvailableProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsAvailable)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToProduct).ToList();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var products = await _context.Products
            .Where(p => p.Category.ToLower() == category.ToLower() && p.IsAvailable)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToProduct).ToList();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        return product != null ? MapToProduct(product) : null;
    }

    public async Task<Product> CreateProductAsync(ProductCreateRequest request)
    {
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            IsSpecial = request.IsSpecial,
            CreatedAt = DateTime.UtcNow,
            IsAvailable = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToProduct(product);
    }

    public async Task<Product> UpdateProductAsync(Guid id, ProductUpdateRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        if (request.Name != null)
            product.Name = request.Name;
        
        if (request.Description != null)
            product.Description = request.Description;
        
        if (request.Price.HasValue)
            product.Price = request.Price.Value;
        
        if (request.Category != null)
            product.Category = request.Category;
        
        if (request.ImageUrl != null)
            product.ImageUrl = request.ImageUrl;
        
        if (request.IsAvailable.HasValue)
            product.IsAvailable = request.IsAvailable.Value;
        
        if (request.IsSpecial.HasValue)
            product.IsSpecial = request.IsSpecial.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProduct(product);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<Product>> GetSpecialProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsSpecial && p.IsAvailable)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToProduct).ToList();
    }

    private static Product MapToProduct(ProductEntity entity)
    {
        return new Product
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Category = entity.Category,
            ImageUrl = entity.ImageUrl,
            IsAvailable = entity.IsAvailable,
            IsSpecial = entity.IsSpecial,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
} 