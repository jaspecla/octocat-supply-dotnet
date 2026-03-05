using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing suppliers
/// </summary>
[ApiController]
[Route("api/suppliers")]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _repository;

    public SuppliersController(ISupplierRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all suppliers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Supplier>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetAll()
    {
        var suppliers = await _repository.GetAllAsync();
        return Ok(suppliers);
    }

    /// <summary>
    /// Get a supplier by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Supplier), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Supplier>> GetById(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        if (supplier == null)
            return NotFound("Supplier not found");

        return Ok(supplier);
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Supplier), StatusCodes.Status201Created)]
    public async Task<ActionResult<Supplier>> Create([FromBody] Supplier supplier)
    {
        var created = await _repository.CreateAsync(supplier);
        return CreatedAtAction(nameof(GetById), new { id = created.SupplierId }, created);
    }

    /// <summary>
    /// Update a supplier by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Supplier), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Supplier>> Update(int id, [FromBody] Supplier supplier)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, supplier);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Supplier not found");
        }
    }

    /// <summary>
    /// Delete a supplier by ID
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
            return NotFound("Supplier not found");
        }
    }

    /// <summary>
    /// Get supplier status by ID
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);
        if (supplier == null)
            return NotFound("Supplier not found");

        var status = ProcessSupplierStatus(supplier);
        return Ok(new { status });
    }

    // Misleading indentation example
    private static string ProcessSupplierStatus(Supplier supplier)
    {
        if (supplier.Active)
            Console.WriteLine("Supplier is active");
            return "APPROVED";

        if (supplier.Verified)
            Console.WriteLine("Supplier verified");
        Console.WriteLine("Setting up account");

        return "PENDING";
    }
}
