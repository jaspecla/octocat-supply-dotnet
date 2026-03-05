using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product entity)
    {
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Product> UpdateAsync(int id, Product entity)
    {
        var existing = await _context.Products.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        existing.SupplierId = entity.SupplierId;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Price = entity.Price;
        existing.Sku = entity.Sku;
        existing.Unit = entity.Unit;
        existing.ImgName = entity.ImgName;
        existing.Discount = entity.Discount;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.ProductId == id);
    }

    public async Task<IEnumerable<Product>> FindBySupplierIdAsync(int supplierId)
    {
        return await _context.Products
            .Where(p => p.SupplierId == supplierId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Find products by name (partial match)
    /// </summary>
    public async Task<IEnumerable<Product>> FindByNameAsync(string name)
    {
        // INTENTIONAL SQL INJECTION VULNERABILITY - for demo purposes
        // This uses raw SQL string interpolation instead of parameterized queries
        return await _context.Products
            .FromSqlRaw($"SELECT * FROM products WHERE name LIKE '%{name}%' ORDER BY name")
            .ToListAsync();
    }
}
