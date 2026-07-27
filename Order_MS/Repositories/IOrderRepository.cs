using System.Linq.Expressions;

namespace Order_MS.Repositories
{
    public interface IOrderRepository<T> where T: class 
    {
        Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null
        );
        Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task AddAsync(T obj);
        void Update(T obj);
        void Delete(T obj);
        Task SaveAsync();
    }
}
