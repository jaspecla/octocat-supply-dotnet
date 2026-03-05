using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class OrderDetailDeliveryRepository : IOrderDetailDeliveryRepository
{
    private readonly AppDbContext _context;

    public OrderDetailDeliveryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDetailDelivery>> GetAllAsync()
    {
        return await _context.OrderDetailDeliveries.ToListAsync();
    }

    public async Task<OrderDetailDelivery?> GetByIdAsync(int id)
    {
        return await _context.OrderDetailDeliveries.FindAsync(id);
    }

    public async Task<OrderDetailDelivery> CreateAsync(OrderDetailDelivery entity)
    {
        _context.OrderDetailDeliveries.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<OrderDetailDelivery> UpdateAsync(int id, OrderDetailDelivery entity)
    {
        var existing = await _context.OrderDetailDeliveries.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"OrderDetailDelivery with ID {id} not found");

        existing.OrderDetailId = entity.OrderDetailId;
        existing.DeliveryId = entity.DeliveryId;
        existing.Quantity = entity.Quantity;
        existing.Notes = entity.Notes;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.OrderDetailDeliveries.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"OrderDetailDelivery with ID {id} not found");

        _context.OrderDetailDeliveries.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.OrderDetailDeliveries.AnyAsync(odd => odd.OrderDetailDeliveryId == id);
    }

    public async Task<IEnumerable<OrderDetailDelivery>> FindByOrderDetailIdAsync(int orderDetailId)
    {
        return await _context.OrderDetailDeliveries
            .Where(odd => odd.OrderDetailId == orderDetailId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDetailDelivery>> FindByDeliveryIdAsync(int deliveryId)
    {
        return await _context.OrderDetailDeliveries
            .Where(odd => odd.DeliveryId == deliveryId)
            .ToListAsync();
    }

    public async Task<int> GetTotalQuantityByOrderDetailIdAsync(int orderDetailId)
    {
        return await _context.OrderDetailDeliveries
            .Where(odd => odd.OrderDetailId == orderDetailId)
            .SumAsync(odd => odd.Quantity);
    }
}
