using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing orders
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;

    public OrdersController(IOrderRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get an order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound("Order not found");

        return Ok(order);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
        var created = await _repository.CreateAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    /// <summary>
    /// Update an order by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> Update(int id, [FromBody] Order order)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, order);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Order not found");
        }
    }

    /// <summary>
    /// Delete an order by ID
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
            return NotFound("Order not found");
        }
    }
}
