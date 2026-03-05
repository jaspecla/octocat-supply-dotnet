using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order> CreateAsync(Order entity)
    {
        _context.Orders.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Order> UpdateAsync(int id, Order entity)
    {
        var existing = await _context.Orders.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Order with ID {id} not found");

        existing.BranchId = entity.BranchId;
        existing.OrderDate = entity.OrderDate;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Status = entity.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Order with ID {id} not found");

        _context.Orders.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Orders.AnyAsync(o => o.OrderId == id);
    }

    public async Task<IEnumerable<Order>> FindByBranchIdAsync(int branchId)
    {
        return await _context.Orders
            .Where(o => o.BranchId == branchId)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> FindByStatusAsync(string status)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> FindByDateRangeAsync(string startDate, string endDate)
    {
        return await _context.Orders
            .Where(o => string.Compare(o.OrderDate, startDate) >= 0 && string.Compare(o.OrderDate, endDate) <= 0)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }
}
