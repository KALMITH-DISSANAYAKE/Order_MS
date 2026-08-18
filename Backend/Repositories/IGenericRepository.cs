namespace Order_MS.Repositories
{
    public interface IGenericRepository<T> where T: class 
{
        Task <IEnumerable<T>> GetAllAsync();
        Task <T?> GetByIdAsync(object id);
    Task AddAsync(T obj);
    void Update(T obj);
    void Delete(T obj);
    Task SaveAsync();
}
}
