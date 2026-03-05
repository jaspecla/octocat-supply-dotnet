using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Branch
{
    [Key]
    [Column("branch_id")]
    [JsonPropertyName("branchId")]
    public int BranchId { get; set; }

    [Column("headquarters_id")]
    [ForeignKey(nameof(Headquarters))]
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

    public Headquarters Headquarters { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
