using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing order detail deliveries
/// </summary>
[ApiController]
[Route("api/order-detail-deliveries")]
[Produces("application/json")]
public class OrderDetailDeliveriesController : ControllerBase
{
    private readonly IOrderDetailDeliveryRepository _repository;

    public OrderDetailDeliveriesController(IOrderDetailDeliveryRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all order detail deliveries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDetailDelivery>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDetailDelivery>>> GetAll()
    {
        var items = await _repository.GetAllAsync();
        return Ok(items);
    }

    /// <summary>
    /// Get an order detail delivery by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDetailDelivery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDelivery>> GetById(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            return NotFound("Order detail delivery not found");

        return Ok(item);
    }

    /// <summary>
    /// Create a new order detail delivery
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailDelivery), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderDetailDelivery>> Create([FromBody] OrderDetailDelivery item)
    {
        var created = await _repository.CreateAsync(item);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderDetailDeliveryId }, created);
    }

    /// <summary>
    /// Update an order detail delivery by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OrderDetailDelivery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDelivery>> Update(int id, [FromBody] OrderDetailDelivery item)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, item);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Order detail delivery not found");
        }
    }

    /// <summary>
    /// Delete an order detail delivery by ID
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
            return NotFound("Order detail delivery not found");
        }
    }
}
