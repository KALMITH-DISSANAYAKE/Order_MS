using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using System.Linq.Expressions;

namespace Order_MS.Repositories
{
    public class OrderRepository<T>: IOrderRepository<T> where T : class
    {
        private readonly OrderMSDbContext _context;
        private readonly DbSet<T> _table;

        public OrderRepository(OrderMSDbContext context)
        {
            _context = context;
            _table = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
           Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _table;

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
                {
                    IQueryable<T> query = _context.Set<T>();

                    if (include != null)
                        query = include(query);

                    return await query.FirstOrDefaultAsync(predicate);
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