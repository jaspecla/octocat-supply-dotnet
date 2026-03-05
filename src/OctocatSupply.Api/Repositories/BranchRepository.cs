using Microsoft.EntityFrameworkCore;
using OctocatSupply.Api.Data;
using OctocatSupply.Api.Models;

namespace OctocatSupply.Api.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly AppDbContext _context;

    public BranchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Branch>> GetAllAsync()
    {
        return await _context.Branches.ToListAsync();
    }

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches.FindAsync(id);
    }

    public async Task<Branch> CreateAsync(Branch entity)
    {
        _context.Branches.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Branch> UpdateAsync(int id, Branch entity)
    {
        var existing = await _context.Branches.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Branch with ID {id} not found");

        existing.HeadquartersId = entity.HeadquartersId;
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Address = entity.Address;
        existing.ContactPerson = entity.ContactPerson;
        existing.Email = entity.Email;
        existing.Phone = entity.Phone;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Branches.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Branch with ID {id} not found");

        _context.Branches.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Branches.AnyAsync(b => b.BranchId == id);
    }

    public async Task<IEnumerable<Branch>> FindByHeadquartersIdAsync(int headquartersId)
    {
        return await _context.Branches
            .Where(b => b.HeadquartersId == headquartersId)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Branch>> FindByNameAsync(string name)
    {
        return await _context.Branches
            .Where(b => b.Name.Contains(name))
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
}
