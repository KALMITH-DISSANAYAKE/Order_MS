using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.Models;

namespace Order_MS.Repositories
{
    public class TransportAssignmentRepository : GenericRepository<TransportAssignment>, ITransportAssignmentRepository
    {
        private readonly OrderMSDbContext _context;

        public TransportAssignmentRepository(OrderMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<TransportAssignment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.TransportAssignments
                .FirstOrDefaultAsync(t => t.order_id == orderId);
        }

        public async Task<TransportAssignment?> GetByOrderIdWithDetailsAsync(int orderId)
        {
            return await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.order_id == orderId);
        }

        public async Task<List<TransportAssignment>> GetAllWithDetailsAsync()
        {
            return await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .ToListAsync();
        }

        public async Task<List<TransportAssignment>> GetDeliveredOrInTransitAsync()
        {
            return await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.status == "Delivered" || t.status == "Completed" || t.Order.order_status == "InTransit")
                .ToListAsync();
        }
    }
}