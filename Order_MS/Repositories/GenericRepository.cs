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
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T obj)
    {
        await _dbSet.AddAsync(obj);
    }

    public void Update(T obj)
    {
        _dbSet.Update(obj);
    }

    public void Delete(T obj)
    {
        _dbSet.Remove(obj);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}