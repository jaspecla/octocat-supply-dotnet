using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class OrderDetail
{
    [Key]
    [Column("order_detail_id")]
    [JsonPropertyName("orderDetailId")]
    public int OrderDetailId { get; set; }

    [Column("order_id")]
    [ForeignKey(nameof(Order))]
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [Column("product_id")]
    [ForeignKey(nameof(Product))]
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [Column("quantity")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [Column("unit_price")]
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column("notes")]
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public ICollection<OrderDetailDelivery> OrderDetailDeliveries { get; set; } = new List<OrderDetailDelivery>();
}
