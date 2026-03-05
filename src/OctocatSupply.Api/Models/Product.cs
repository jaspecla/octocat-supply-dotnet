using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Product
{
    [Key]
    [Column("product_id")]
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [Column("supplier_id")]
    [ForeignKey(nameof(Supplier))]
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

    [Column("price")]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [Required]
    [Column("sku")]
    [JsonPropertyName("sku")]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Column("unit")]
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [Required]
    [Column("img_name")]
    [JsonPropertyName("imgName")]
    public string ImgName { get; set; } = string.Empty;

    [Column("discount")]
    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
