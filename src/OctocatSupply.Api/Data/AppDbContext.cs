using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Headquarters> Headquarters => Set<Headquarters>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<OrderDetailDelivery> OrderDetailDeliveries => Set<OrderDetailDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table names (snake_case)
        modelBuilder.Entity<Supplier>().ToTable("suppliers");
        modelBuilder.Entity<Headquarters>().ToTable("headquarters");
        modelBuilder.Entity<Branch>().ToTable("branches");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OrderDetail>().ToTable("order_details");
        modelBuilder.Entity<Delivery>().ToTable("deliveries");
        modelBuilder.Entity<OrderDetailDelivery>().ToTable("order_detail_deliveries");

        // Decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Discount)
            .HasPrecision(5, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasPrecision(10, 2);

        // FK relationships with cascade delete
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Headquarters)
            .WithMany(h => h.Branches)
            .HasForeignKey(b => b.HeadquartersId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Branch)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Supplier)
            .WithMany(s => s.Deliveries)
            .HasForeignKey(d => d.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetailDelivery>()
            .HasOne(odd => odd.OrderDetail)
            .WithMany(od => od.OrderDetailDeliveries)
            .HasForeignKey(odd => odd.OrderDetailId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetailDelivery>()
            .HasOne(odd => odd.Delivery)
            .WithMany(d => d.OrderDetailDeliveries)
            .HasForeignKey(odd => odd.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        modelBuilder.Entity<Branch>()
            .HasIndex(b => b.HeadquartersId)
            .HasDatabaseName("idx_branches_headquarters_id");

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SupplierId)
            .HasDatabaseName("idx_products_supplier_id");

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku)
            .HasDatabaseName("idx_products_sku");

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.BranchId)
            .HasDatabaseName("idx_orders_branch_id");

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status)
            .HasDatabaseName("idx_orders_status");

        modelBuilder.Entity<OrderDetail>()
            .HasIndex(od => od.OrderId)
            .HasDatabaseName("idx_order_details_order_id");

        modelBuilder.Entity<OrderDetail>()
            .HasIndex(od => od.ProductId)
            .HasDatabaseName("idx_order_details_product_id");

        modelBuilder.Entity<Delivery>()
            .HasIndex(d => d.SupplierId)
            .HasDatabaseName("idx_deliveries_supplier_id");

        modelBuilder.Entity<Delivery>()
            .HasIndex(d => d.Status)
            .HasDatabaseName("idx_deliveries_status");

        modelBuilder.Entity<OrderDetailDelivery>()
            .HasIndex(odd => odd.OrderDetailId)
            .HasDatabaseName("idx_order_detail_deliveries_order_detail_id");

        modelBuilder.Entity<OrderDetailDelivery>()
            .HasIndex(odd => odd.DeliveryId)
            .HasDatabaseName("idx_order_detail_deliveries_delivery_id");
    }
}
