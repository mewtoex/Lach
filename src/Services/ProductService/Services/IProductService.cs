using Lach.Shared.Common.Models;

namespace ProductService.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<List<Product>> GetAvailableProductsAsync();
    Task<List<Product>> GetProductsByCategoryAsync(string category);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product> CreateProductAsync(ProductCreateRequest request);
    Task<Product> UpdateProductAsync(Guid id, ProductUpdateRequest request);
    Task<bool> DeleteProductAsync(Guid id);
    Task<List<Product>> GetSpecialProductsAsync();
} 