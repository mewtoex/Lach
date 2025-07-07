using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;
using ProductService.Models;

namespace ProductService.Services;

public class AddOnCategoryService : IAddOnCategoryService
{
    private readonly ProductDbContext _context;

    public AddOnCategoryService(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<AddOnCategoryDto>> GetAllAsync()
    {
        var categories = await _context.AddOnCategories
            .Include(c => c.AddOns)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<AddOnCategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _context.AddOnCategories
            .Include(c => c.AddOns)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        return category != null ? MapToDto(category) : null;
    }

    public async Task<AddOnCategoryDto> CreateAsync(CreateAddOnCategoryDto dto)
    {
        var category = new AddOnCategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Color = dto.Color,
            Icon = dto.Icon,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            MaxSelections = dto.MaxSelections,
            IsRequired = dto.IsRequired,
            MinPrice = dto.MinPrice,
            MaxPrice = dto.MaxPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.AddOnCategories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<AddOnCategoryDto?> UpdateAsync(Guid id, UpdateAddOnCategoryDto dto)
    {
        var category = await _context.AddOnCategories.FindAsync(id);
        if (category == null) return null;

        if (dto.Name != null) category.Name = dto.Name;
        if (dto.Description != null) category.Description = dto.Description;
        if (dto.ImageUrl != null) category.ImageUrl = dto.ImageUrl;
        if (dto.Color != null) category.Color = dto.Color;
        if (dto.Icon != null) category.Icon = dto.Icon;
        if (dto.IsActive.HasValue) category.IsActive = dto.IsActive.Value;
        if (dto.DisplayOrder.HasValue) category.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.MaxSelections.HasValue) category.MaxSelections = dto.MaxSelections.Value;
        if (dto.IsRequired.HasValue) category.IsRequired = dto.IsRequired.Value;
        if (dto.MinPrice.HasValue) category.MinPrice = dto.MinPrice;
        if (dto.MaxPrice.HasValue) category.MaxPrice = dto.MaxPrice;

        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.AddOnCategories.FindAsync(id);
        if (category == null) return false;

        // Verificar se há produtos usando esta categoria
        var hasProducts = await _context.ProductAddOnCategories
            .AnyAsync(pac => pac.AddOnCategoryId == id && pac.IsActive);

        if (hasProducts)
        {
            // Soft delete - apenas desativar
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete - remover completamente
            _context.AddOnCategories.Remove(category);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AddOnCategoryDto>> GetByProductIdAsync(Guid productId)
    {
        var categories = await _context.ProductAddOnCategories
            .Include(pac => pac.AddOnCategory)
            .ThenInclude(ac => ac.AddOns)
            .Where(pac => pac.ProductId == productId && pac.IsActive && pac.AddOnCategory.IsActive)
            .OrderBy(pac => pac.DisplayOrder)
            .ThenBy(pac => pac.AddOnCategory.Name)
            .Select(pac => pac.AddOnCategory)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<ProductAddOnCategoryDto> AddCategoryToProductAsync(CreateProductAddOnCategoryDto dto)
    {
        // Verificar se o produto existe
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            throw new ArgumentException("Produto não encontrado");

        // Verificar se a categoria existe
        var category = await _context.AddOnCategories.FindAsync(dto.AddOnCategoryId);
        if (category == null)
            throw new ArgumentException("Categoria de adicional não encontrada");

        // Verificar se já existe o relacionamento
        var existing = await _context.ProductAddOnCategories
            .FirstOrDefaultAsync(pac => pac.ProductId == dto.ProductId && pac.AddOnCategoryId == dto.AddOnCategoryId);

        if (existing != null)
        {
            // Ativar se estava desativado
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToProductAddOnCategoryDto(existing);
        }

        var productAddOnCategory = new ProductAddOnCategoryEntity
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            AddOnCategoryId = dto.AddOnCategoryId,
            MaxSelections = dto.MaxSelections ?? category.MaxSelections,
            IsRequired = dto.IsRequired ?? category.IsRequired,
            DisplayOrder = dto.DisplayOrder ?? 0,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductAddOnCategories.Add(productAddOnCategory);
        await _context.SaveChangesAsync();

        return MapToProductAddOnCategoryDto(productAddOnCategory);
    }

    public async Task<bool> RemoveCategoryFromProductAsync(Guid productId, Guid addOnCategoryId)
    {
        var productAddOnCategory = await _context.ProductAddOnCategories
            .FirstOrDefaultAsync(pac => pac.ProductId == productId && pac.AddOnCategoryId == addOnCategoryId);

        if (productAddOnCategory == null) return false;

        productAddOnCategory.IsActive = false;
        productAddOnCategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ProductAddOnCategoryDto?> UpdateProductAddOnCategoryAsync(Guid productId, Guid addOnCategoryId, UpdateProductAddOnCategoryDto dto)
    {
        var productAddOnCategory = await _context.ProductAddOnCategories
            .Include(pac => pac.AddOnCategory)
            .FirstOrDefaultAsync(pac => pac.ProductId == productId && pac.AddOnCategoryId == addOnCategoryId);

        if (productAddOnCategory == null) return null;

        if (dto.MaxSelections.HasValue) productAddOnCategory.MaxSelections = dto.MaxSelections.Value;
        if (dto.IsRequired.HasValue) productAddOnCategory.IsRequired = dto.IsRequired.Value;
        if (dto.DisplayOrder.HasValue) productAddOnCategory.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsActive.HasValue) productAddOnCategory.IsActive = dto.IsActive.Value;

        productAddOnCategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToProductAddOnCategoryDto(productAddOnCategory);
    }

    private static AddOnCategoryDto MapToDto(AddOnCategoryEntity entity)
    {
        return new AddOnCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Color = entity.Color,
            Icon = entity.Icon,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder,
            MaxSelections = entity.MaxSelections,
            IsRequired = entity.IsRequired,
            MinPrice = entity.MinPrice,
            MaxPrice = entity.MaxPrice,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AddOns = entity.AddOns?.Select(ao => new ProductAddOnDto
            {
                Id = ao.Id,
                Name = ao.Name,
                Description = ao.Description,
                Price = ao.Price,
                ImageUrl = ao.ImageUrl,
                IsAvailable = ao.IsAvailable,
                MaxQuantity = ao.MaxQuantity,
                AddOnCategoryId = ao.AddOnCategoryId,
                ProductId = ao.ProductId,
                CreatedAt = ao.CreatedAt,
                UpdatedAt = ao.UpdatedAt
            }).ToList() ?? new List<ProductAddOnDto>()
        };
    }

    private static ProductAddOnCategoryDto MapToProductAddOnCategoryDto(ProductAddOnCategoryEntity entity)
    {
        return new ProductAddOnCategoryDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            AddOnCategoryId = entity.AddOnCategoryId,
            MaxSelections = entity.MaxSelections,
            IsRequired = entity.IsRequired,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            AddOnCategory = entity.AddOnCategory != null ? MapToDto(entity.AddOnCategory) : null!,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
} 