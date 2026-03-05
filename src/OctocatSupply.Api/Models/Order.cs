using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OctocatSupply.Api.Models;

public class Order
{
    [Key]
    [Column("order_id")]
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [Column("branch_id")]
    [ForeignKey(nameof(Branch))]
    [JsonPropertyName("branchId")]
    public int BranchId { get; set; }

    [Required]
    [Column("order_date")]
    [JsonPropertyName("orderDate")]
    public string OrderDate { get; set; } = string.Empty;

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

    public Branch Branch { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
