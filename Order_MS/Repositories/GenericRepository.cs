using Microsoft.EntityFrameworkCore;
using Order_MS.Data;

namespace Order_MS.Repositories
{
    public class GenericRepository<T>: IGenericRepository<T> where T : class
    {
        private readonly OrderMSDbContext _context;
        private readonly DbSet<T> _table;

        public GenericRepository(OrderMSDbContext context)
        {
            _context = context;
            _table = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _table.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _table.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _table.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _table.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

        }

        public void Delete(T entity)
        {
            _table.Remove(entity);
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}