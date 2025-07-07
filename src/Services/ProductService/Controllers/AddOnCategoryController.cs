using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddOnCategoryController : ControllerBase
{
    private readonly IAddOnCategoryService _addOnCategoryService;

    public AddOnCategoryController(IAddOnCategoryService addOnCategoryService)
    {
        _addOnCategoryService = addOnCategoryService;
    }

    /// <summary>
    /// Lista todas as categorias de adicionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AddOnCategoryDto>>> GetAll()
    {
        var categories = await _addOnCategoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Obtém uma categoria de adicional por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddOnCategoryDto>> GetById(Guid id)
    {
        var category = await _addOnCategoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Cria uma nova categoria de adicional
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddOnCategoryDto>> Create(CreateAddOnCategoryDto dto)
    {
        var category = await _addOnCategoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Atualiza uma categoria de adicional
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddOnCategoryDto>> Update(Guid id, UpdateAddOnCategoryDto dto)
    {
        var category = await _addOnCategoryService.UpdateAsync(id, dto);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Remove uma categoria de adicional
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _addOnCategoryService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Lista categorias de adicionais de um produto
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<List<AddOnCategoryDto>>> GetByProductId(Guid productId)
    {
        var categories = await _addOnCategoryService.GetByProductIdAsync(productId);
        return Ok(categories);
    }

    /// <summary>
    /// Adiciona uma categoria de adicional a um produto
    /// </summary>
    [HttpPost("product")]
    public async Task<ActionResult<ProductAddOnCategoryDto>> AddCategoryToProduct(CreateProductAddOnCategoryDto dto)
    {
        try
        {
            var productAddOnCategory = await _addOnCategoryService.AddCategoryToProductAsync(dto);
            return CreatedAtAction(nameof(GetByProductId), new { productId = dto.ProductId }, productAddOnCategory);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove uma categoria de adicional de um produto
    /// </summary>
    [HttpDelete("product/{productId:guid}/category/{addOnCategoryId:guid}")]
    public async Task<ActionResult> RemoveCategoryFromProduct(Guid productId, Guid addOnCategoryId)
    {
        var success = await _addOnCategoryService.RemoveCategoryFromProductAsync(productId, addOnCategoryId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Atualiza configurações de uma categoria de adicional em um produto
    /// </summary>
    [HttpPut("product/{productId:guid}/category/{addOnCategoryId:guid}")]
    public async Task<ActionResult<ProductAddOnCategoryDto>> UpdateProductAddOnCategory(
        Guid productId, 
        Guid addOnCategoryId, 
        UpdateProductAddOnCategoryDto dto)
    {
        var productAddOnCategory = await _addOnCategoryService.UpdateProductAddOnCategoryAsync(
            productId, addOnCategoryId, dto);
        
        if (productAddOnCategory == null)
            return NotFound();

        return Ok(productAddOnCategory);
    }
} 