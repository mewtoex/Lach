using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Lista todos os clientes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Obtém um cliente por email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<ActionResult<CustomerDto>> GetByEmail(string email)
    {
        var customer = await _customerService.GetByEmailAsync(email);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        var customer = await _customerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// Atualiza um cliente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _customerService.UpdateAsync(id, dto);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    /// <summary>
    /// Remove um cliente
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _customerService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Lista endereços de um cliente
    /// </summary>
    [HttpGet("{customerId:guid}/addresses")]
    public async Task<ActionResult<List<CustomerAddressDto>>> GetAddresses(Guid customerId)
    {
        var addresses = await _customerService.GetAddressesAsync(customerId);
        return Ok(addresses);
    }

    /// <summary>
    /// Adiciona um endereço ao cliente
    /// </summary>
    [HttpPost("{customerId:guid}/addresses")]
    public async Task<ActionResult<CustomerAddressDto>> AddAddress(Guid customerId, CreateCustomerAddressDto dto)
    {
        if (dto.CustomerId != customerId)
            return BadRequest("CustomerId não corresponde");

        var address = await _customerService.AddAddressAsync(dto);
        return CreatedAtAction(nameof(GetAddresses), new { customerId }, address);
    }

    /// <summary>
    /// Atualiza um endereço do cliente
    /// </summary>
    [HttpPut("{customerId:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult<CustomerAddressDto>> UpdateAddress(
        Guid customerId, 
        Guid addressId, 
        UpdateCustomerAddressDto dto)
    {
        var address = await _customerService.UpdateAddressAsync(customerId, addressId, dto);
        if (address == null)
            return NotFound();

        return Ok(address);
    }

    /// <summary>
    /// Remove um endereço do cliente
    /// </summary>
    [HttpDelete("{customerId:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult> RemoveAddress(Guid customerId, Guid addressId)
    {
        var success = await _customerService.RemoveAddressAsync(customerId, addressId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Define um endereço como padrão
    /// </summary>
    [HttpPut("{customerId:guid}/addresses/{addressId:guid}/default")]
    public async Task<ActionResult> SetDefaultAddress(Guid customerId, Guid addressId)
    {
        var success = await _customerService.SetDefaultAddressAsync(customerId, addressId);
        if (!success)
            return NotFound();

        return NoContent();
    }
} 