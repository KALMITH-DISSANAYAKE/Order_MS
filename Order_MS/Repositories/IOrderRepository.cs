using Order_MS.Models;

namespace Order_MS.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
    }
}