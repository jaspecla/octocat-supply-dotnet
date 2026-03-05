using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing deliveries
/// </summary>
[ApiController]
[Route("api/deliveries")]
[Produces("application/json")]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryRepository _repository;

    public DeliveriesController(IDeliveryRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all deliveries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Delivery>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Delivery>>> GetAll()
    {
        var deliveries = await _repository.GetAllAsync();
        return Ok(deliveries);
    }

    /// <summary>
    /// Get a delivery by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Delivery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Delivery>> GetById(int id)
    {
        var delivery = await _repository.GetByIdAsync(id);
        if (delivery == null)
            return NotFound("Delivery not found");

        return Ok(delivery);
    }

    /// <summary>
    /// Create a new delivery
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Delivery), StatusCodes.Status201Created)]
    public async Task<ActionResult<Delivery>> Create([FromBody] Delivery delivery)
    {
        var created = await _repository.CreateAsync(delivery);
        return CreatedAtAction(nameof(GetById), new { id = created.DeliveryId }, created);
    }

    /// <summary>
    /// Update the status of a delivery
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(Delivery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] DeliveryStatusUpdate statusUpdate)
    {
        try
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return NotFound("Delivery not found");

            var updatedDelivery = await _repository.UpdateStatusAsync(id, statusUpdate.Status);

            if (!string.IsNullOrEmpty(statusUpdate.DeliveryPartner))
            {
                // INTENTIONAL COMMAND INJECTION VULNERABILITY - for demo purposes
                // User-supplied deliveryPartner string is directly interpolated into a shell command
                var process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c notify {statusUpdate.DeliveryPartner}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;

                try
                {
                    process.Start();
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    return Ok(new { delivery = updatedDelivery, commandOutput = output });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error executing command: {ex}");
                    return StatusCode(500, new { error = ex.Message });
                }
            }

            return Ok(updatedDelivery);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Delivery not found");
        }
    }

    /// <summary>
    /// Update a delivery by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Delivery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Delivery>> Update(int id, [FromBody] Delivery delivery)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, delivery);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Delivery not found");
        }
    }

    /// <summary>
    /// Delete a delivery by ID
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
            return NotFound("Delivery not found");
        }
    }
}

public class DeliveryStatusUpdate
{
    public string Status { get; set; } = string.Empty;
    public string? DeliveryPartner { get; set; }
}
