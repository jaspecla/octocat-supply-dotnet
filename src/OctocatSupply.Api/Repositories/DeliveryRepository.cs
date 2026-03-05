using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly AppDbContext _context;

    public DeliveryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Delivery>> GetAllAsync()
    {
        return await _context.Deliveries.ToListAsync();
    }

    public async Task<Delivery?> GetByIdAsync(int id)
    {
        return await _context.Deliveries.FindAsync(id);
    }

    public async Task<Delivery> CreateAsync(Delivery entity)
    {
        _context.Deliveries.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Delivery> UpdateAsync(int id, Delivery entity)
    {
        var existing = await _context.Deliveries.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Delivery with ID {id} not found");

        existing.SupplierId = entity.SupplierId;
        existing.DeliveryDate = entity.DeliveryDate;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Status = entity.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Deliveries.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Delivery with ID {id} not found");

        _context.Deliveries.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Deliveries.AnyAsync(d => d.DeliveryId == id);
    }

    public async Task<IEnumerable<Delivery>> FindBySupplierIdAsync(int supplierId)
    {
        return await _context.Deliveries
            .Where(d => d.SupplierId == supplierId)
            .OrderBy(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Delivery>> FindByStatusAsync(string status)
    {
        return await _context.Deliveries
            .Where(d => d.Status == status)
            .OrderBy(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Delivery>> FindByDateRangeAsync(string startDate, string endDate)
    {
        return await _context.Deliveries
            .Where(d => string.Compare(d.DeliveryDate, startDate) >= 0 && string.Compare(d.DeliveryDate, endDate) <= 0)
            .OrderBy(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<Delivery> UpdateStatusAsync(int id, string status)
    {
        var existing = await _context.Deliveries.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Delivery with ID {id} not found");

        existing.Status = status;
        await _context.SaveChangesAsync();
        return existing;
    }
}
