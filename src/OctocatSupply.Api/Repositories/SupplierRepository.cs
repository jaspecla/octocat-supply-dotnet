using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _context;

    public SupplierRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers.ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _context.Suppliers.FindAsync(id);
    }

    public async Task<Supplier> CreateAsync(Supplier entity)
    {
        _context.Suppliers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Supplier> UpdateAsync(int id, Supplier entity)
    {
        var existing = await _context.Suppliers.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Supplier with ID {id} not found");

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.ContactPerson = entity.ContactPerson;
        existing.Email = entity.Email;
        existing.Phone = entity.Phone;
        existing.Active = entity.Active;
        existing.Verified = entity.Verified;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Suppliers.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Supplier with ID {id} not found");

        _context.Suppliers.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Suppliers.AnyAsync(s => s.SupplierId == id);
    }

    public async Task<IEnumerable<Supplier>> FindByNameAsync(string name)
    {
        return await _context.Suppliers
            .Where(s => s.Name.Contains(name))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}
