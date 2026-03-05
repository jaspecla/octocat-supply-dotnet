using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class HeadquartersRepository : IHeadquartersRepository
{
    private readonly AppDbContext _context;

    public HeadquartersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Headquarters>> GetAllAsync()
    {
        return await _context.Headquarters.ToListAsync();
    }

    public async Task<Headquarters?> GetByIdAsync(int id)
    {
        return await _context.Headquarters.FindAsync(id);
    }

    public async Task<Headquarters> CreateAsync(Headquarters entity)
    {
        _context.Headquarters.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Headquarters> UpdateAsync(int id, Headquarters entity)
    {
        var existing = await _context.Headquarters.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Headquarters with ID {id} not found");

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Address = entity.Address;
        existing.ContactPerson = entity.ContactPerson;
        existing.Email = entity.Email;
        existing.Phone = entity.Phone;
        existing.City = entity.City;
        existing.Country = entity.Country;
        existing.FloorCount = entity.FloorCount;
        existing.Capacity = entity.Capacity;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Headquarters.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Headquarters with ID {id} not found");

        _context.Headquarters.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Headquarters.AnyAsync(h => h.HeadquartersId == id);
    }

    public async Task<IEnumerable<Headquarters>> FindByNameAsync(string name)
    {
        return await _context.Headquarters
            .Where(h => h.Name.Contains(name))
            .OrderBy(h => h.Name)
            .ToListAsync();
    }
}
