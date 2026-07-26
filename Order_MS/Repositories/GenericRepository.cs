using Microsoft.EntityFrameworkCore;
using Order_MS.Data;

namespace Order_MS.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly OrderMSDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(OrderMSDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();  // Gets DbSet<Branch>, DbSet<Item>, etc.
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }
}