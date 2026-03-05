using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly AppDbContext _context;

    public OrderDetailRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDetail>> GetAllAsync()
    {
        return await _context.OrderDetails.ToListAsync();
    }

    public async Task<OrderDetail?> GetByIdAsync(int id)
    {
        return await _context.OrderDetails.FindAsync(id);
    }

    public async Task<OrderDetail> CreateAsync(OrderDetail entity)
    {
        _context.OrderDetails.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<OrderDetail> UpdateAsync(int id, OrderDetail entity)
    {
        var existing = await _context.OrderDetails.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"OrderDetail with ID {id} not found");

        existing.OrderId = entity.OrderId;
        existing.ProductId = entity.ProductId;
        existing.Quantity = entity.Quantity;
        existing.UnitPrice = entity.UnitPrice;
        existing.Notes = entity.Notes;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.OrderDetails.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"OrderDetail with ID {id} not found");

        _context.OrderDetails.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.OrderDetails.AnyAsync(od => od.OrderDetailId == id);
    }

    public async Task<IEnumerable<OrderDetail>> FindByOrderIdAsync(int orderId)
    {
        return await _context.OrderDetails
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDetail>> FindByProductIdAsync(int productId)
    {
        return await _context.OrderDetails
            .Where(od => od.ProductId == productId)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalValueByOrderIdAsync(int orderId)
    {
        return await _context.OrderDetails
            .Where(od => od.OrderId == orderId)
            .SumAsync(od => od.Quantity * od.UnitPrice);
    }
}
