using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing order details
/// </summary>
[ApiController]
[Route("api/order-details")]
[Produces("application/json")]
public class OrderDetailsController : ControllerBase
{
    private readonly IOrderDetailRepository _repository;

    public OrderDetailsController(IOrderDetailRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all order details
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDetail>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDetail>>> GetAll()
    {
        var orderDetails = await _repository.GetAllAsync();
        return Ok(orderDetails);
    }

    /// <summary>
    /// Get an order detail by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetail>> GetById(int id)
    {
        var orderDetail = await _repository.GetByIdAsync(id);
        if (orderDetail == null)
            return NotFound("Order detail not found");

        return Ok(orderDetail);
    }

    /// <summary>
    /// Create a new order detail
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderDetail>> Create([FromBody] OrderDetail orderDetail)
    {
        var created = await _repository.CreateAsync(orderDetail);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderDetailId }, created);
    }

    /// <summary>
    /// Update an order detail by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetail>> Update(int id, [FromBody] OrderDetail orderDetail)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, orderDetail);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Order detail not found");
        }
    }

    /// <summary>
    /// Delete an order detail by ID
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
            return NotFound("Order detail not found");
        }
    }
}
