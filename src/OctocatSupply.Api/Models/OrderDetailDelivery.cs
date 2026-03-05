using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class OrderDetailDelivery
{
    [Key]
    [Column("order_detail_delivery_id")]
    [JsonPropertyName("orderDetailDeliveryId")]
    public int OrderDetailDeliveryId { get; set; }

    [Column("order_detail_id")]
    [ForeignKey(nameof(OrderDetail))]
    [JsonPropertyName("orderDetailId")]
    public int OrderDetailId { get; set; }

    [Column("delivery_id")]
    [ForeignKey(nameof(Delivery))]
    [JsonPropertyName("deliveryId")]
    public int DeliveryId { get; set; }

    [Column("quantity")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("notes")]
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    public OrderDetail OrderDetail { get; set; } = null!;

    public Delivery Delivery { get; set; } = null!;
}
