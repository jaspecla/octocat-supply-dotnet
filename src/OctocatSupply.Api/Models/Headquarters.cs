using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Headquarters
{
    [Key]
    [Column("headquarters_id")]
    [JsonPropertyName("headquartersId")]
    public int HeadquartersId { get; set; }

    [Required]
    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("address")]
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Column("contact_person")]
    [JsonPropertyName("contactPerson")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("phone")]
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("city")]
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [Column("country")]
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [Column("floor_count")]
    [JsonPropertyName("floorCount")]
    public int? FloorCount { get; set; }

    [Column("capacity")]
    [JsonPropertyName("capacity")]
    public int? Capacity { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
