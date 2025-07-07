using ProductService.Models;

namespace ProductService.Services;

public interface IAddOnCategoryService
{
    Task<List<AddOnCategoryDto>> GetAllAsync();
    Task<AddOnCategoryDto?> GetByIdAsync(Guid id);
    Task<AddOnCategoryDto> CreateAsync(CreateAddOnCategoryDto dto);
    Task<AddOnCategoryDto?> UpdateAsync(Guid id, UpdateAddOnCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<AddOnCategoryDto>> GetByProductIdAsync(Guid productId);
    Task<ProductAddOnCategoryDto> AddCategoryToProductAsync(CreateProductAddOnCategoryDto dto);
    Task<bool> RemoveCategoryFromProductAsync(Guid productId, Guid addOnCategoryId);
    Task<ProductAddOnCategoryDto?> UpdateProductAddOnCategoryAsync(Guid productId, Guid addOnCategoryId, UpdateProductAddOnCategoryDto dto);
} 