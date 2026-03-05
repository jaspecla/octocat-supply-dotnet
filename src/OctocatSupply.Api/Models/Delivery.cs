using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Delivery
{
    [Key]
    [Column("delivery_id")]
    [JsonPropertyName("deliveryId")]
    public int DeliveryId { get; set; }

    [Column("supplier_id")]
    [ForeignKey(nameof(Supplier))]
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }

    [Required]
    [Column("delivery_date")]
    [JsonPropertyName("deliveryDate")]
    public string DeliveryDate { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    public Supplier Supplier { get; set; } = null!;

    public ICollection<OrderDetailDelivery> OrderDetailDeliveries { get; set; } = new List<OrderDetailDelivery>();
}
