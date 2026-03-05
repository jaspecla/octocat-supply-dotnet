using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Supplier
{
    [Key]
    [Column("supplier_id")]
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }

    [Required]
    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

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

    [Column("active")]
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [Column("verified")]
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
