using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.Models;

namespace Order_MS.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly OrderMSDbContext _context;

        public OrderRepository(OrderMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            if (int.TryParse(orderNumber, out int id))
            {
                return await _context.Orders
                    .FirstOrDefaultAsync(o => o.order_id == id);
            }
            return null;
        }
    }
}