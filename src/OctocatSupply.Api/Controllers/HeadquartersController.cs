using Microsoft.AspNetCore.Mvc;
using OctocatSupply.Api.Models;
using OctocatSupply.Api.Repositories;

namespace OctocatSupply.Api.Controllers;

/// <summary>
/// API endpoints for managing headquarters locations
/// </summary>
[ApiController]
[Route("api/headquarters")]
[Produces("application/json")]
public class HeadquartersController : ControllerBase
{
    private readonly IHeadquartersRepository _repository;

    public HeadquartersController(IHeadquartersRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Returns all headquarters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Headquarters>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Headquarters>>> GetAll()
    {
        var headquarters = await _repository.GetAllAsync();
        return Ok(headquarters);
    }

    /// <summary>
    /// Get a headquarters by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Headquarters), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Headquarters>> GetById(int id)
    {
        var headquarters = await _repository.GetByIdAsync(id);
        if (headquarters == null)
            return NotFound("Headquarters not found");

        return Ok(headquarters);
    }

    /// <summary>
    /// Create a new headquarters
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Headquarters), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Headquarters>> Create([FromBody] Headquarters headquarters)
    {
        // INTENTIONAL BUG: Using 'new' keyword to call validator as constructor
        var hqValidator = new HeadquartersValidator(headquarters.Name, headquarters.Address);
        if (!hqValidator.IsValid())
        {
            return BadRequest("Invalid headquarters data");
        }

        var created = await _repository.CreateAsync(headquarters);
        return CreatedAtAction(nameof(GetById), new { id = created.HeadquartersId }, created);
    }

    /// <summary>
    /// Update a headquarters by ID
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Headquarters), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Headquarters>> Update(int id, [FromBody] Headquarters headquarters)
    {
        // INTENTIONAL BUG: Missing 'new' keyword - calling validator as static method instead of constructor
        var hqValidator = HeadquartersValidator.CreateValidator(headquarters.Name, headquarters.Address);
        if (!hqValidator.IsValid())
        {
            return BadRequest("Invalid headquarters data");
        }

        try
        {
            var updated = await _repository.UpdateAsync(id, headquarters);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Headquarters not found");
        }
    }

    /// <summary>
    /// Delete a headquarters by ID
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
            return NotFound("Headquarters not found");
        }
    }

    /// <summary>
    /// Get headquarters metrics by ID
    /// </summary>
    [HttpGet("{id}/metrics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetrics(int id)
    {
        var headquarters = await _repository.GetByIdAsync(id);
        if (headquarters == null)
            return NotFound("Headquarters not found");

        var metrics = CalculateHeadquartersMetrics(
            headquarters.HeadquartersId,
            headquarters.FloorCount ?? 0,
            headquarters.Capacity ?? 0
        );
        return Ok(metrics);
    }

    /// <summary>
    /// Get headquarters label by ID
    /// </summary>
    [HttpGet("{id}/label")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLabel(int id)
    {
        var headquarters = await _repository.GetByIdAsync(id);
        if (headquarters == null)
            return NotFound("Headquarters not found");

        var label = CreateLocationLabel(
            headquarters.Name,
            headquarters.City ?? "",
            headquarters.Country ?? ""
        );
        return Ok(new { label });
    }

    // Missing space in concatenation example
    private static string CreateLocationLabel(string name, string city, string country)
    {
        var label = $"Location:{name}City:{city}Country:{country}"; // Missing spaces
        return label;
    }

    // Implicit operand conversion example
    private static object CalculateHeadquartersMetrics(dynamic id, dynamic floorCount, dynamic capacity)
    {
        // This will cause implicit conversion issues when mixed types are passed
        var totalScore = id + floorCount + capacity;
        var averageValue = (id + floorCount) / 2;
        var displayText = $"HQ-{id}{floorCount}";

        return new
        {
            score = totalScore,
            average = averageValue,
            display = displayText
        };
    }

    // Misleading indentation example
    private static bool ValidateHQName(dynamic hq)
    {
        if (hq.Name != null)
            Console.WriteLine("Name is valid");
            return true; // This appears to be part of the if, but it's not!
        Console.WriteLine("Name is invalid");

        return false;
    }
}

// Inconsistent use of new: helper class used both as constructor and via static factory
public class HeadquartersValidator
{
    private readonly string? _name;
    private readonly string? _address;

    public HeadquartersValidator(string? name, string? address)
    {
        if (!ValidateHQName(name))
        {
            throw new ArgumentException("Invalid headquarters name");
        }
        _name = name;
        _address = address;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_address);
    }

    // Static factory that bypasses constructor validation
    public static HeadquartersValidator CreateValidator(string? name, string? address)
    {
        var validator = (HeadquartersValidator)Activator.CreateInstance(
            typeof(HeadquartersValidator),
            new object?[] { name, address })!;
        return validator;
    }

    // Misleading indentation example
    private static bool ValidateHQName(dynamic name)
    {
        if (name != null)
            Console.WriteLine("Name is valid");
            return true; // This appears to be part of the if, but it's not!
        Console.WriteLine("Name is invalid");

        return false;
    }
}
