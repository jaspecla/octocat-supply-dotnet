using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing products
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound("Product not found");

        return Ok(product);
    }

    /// <summary>
    /// Get products by name (partial match)
    /// </summary>
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Product>>> GetByName(string name)
    {
        // This passes user input directly to the repository's vulnerable FindByName method
        var products = await _repository.FindByNameAsync(name);
        if (products == null || !products.Any())
            return NotFound("Product not found");

        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        var created = await _repository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
    }

    /// <summary>
    /// Update a product by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, product);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Product not found");
        }
    }

    /// <summary>
    /// Delete a product by ID
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Product not found");
        }
    }
}
