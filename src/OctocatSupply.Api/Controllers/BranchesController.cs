using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing branches
/// </summary>
[ApiController]
[Route("api/branches")]
[Produces("application/json")]
public class BranchesController : ControllerBase
{
    private readonly IBranchRepository _repository;

    public BranchesController(IBranchRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all branches
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Branch>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Branch>>> GetAll()
    {
        var branches = await _repository.GetAllAsync();
        return Ok(branches);
    }

    /// <summary>
    /// Get a branch by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Branch), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Branch>> GetById(int id)
    {
        var branch = await _repository.GetByIdAsync(id);
        if (branch == null)
            return NotFound("Branch not found");

        return Ok(branch);
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Branch), StatusCodes.Status201Created)]
    public async Task<ActionResult<Branch>> Create([FromBody] Branch branch)
    {
        var created = await _repository.CreateAsync(branch);
        return CreatedAtAction(nameof(GetById), new { id = created.BranchId }, created);
    }

    /// <summary>
    /// Update a branch by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Branch), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Branch>> Update(int id, [FromBody] Branch branch)
    {
        try
        {
            var updated = await _repository.UpdateAsync(id, branch);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Branch not found");
        }
    }

    /// <summary>
    /// Delete a branch by ID
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
            return NotFound("Branch not found");
        }
    }
}
