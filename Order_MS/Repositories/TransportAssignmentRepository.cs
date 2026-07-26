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

        public async Task<List<TransportAssignment>> GetAllByOrderIdAsync(int orderId)
        {
            return await _context.TransportAssignments
                .Where(t => t.order_id == orderId)
                .ToListAsync();
        }

        public async Task<List<TransportAssignment>> GetAllByOrderIdWithDetailsAsync(int orderId)
        {
            return await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.order_id == orderId)
                .ToListAsync();
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

        public async Task<List<TransportAssignment>> GetActiveByVehicleIdAsync(int vehicleId)
        {
            return await _context.TransportAssignments
                .Where(t => t.vehicle_id == vehicleId && t.status != "Delivered" && t.status != "Completed" && t.status != "Cancelled")
                .ToListAsync();
        }

        public async Task<List<TransportAssignment>> GetActiveByDriverIdAsync(int driverId)
        {
            return await _context.TransportAssignments
                .Where(t => t.driver_id == driverId && t.status != "Delivered" && t.status != "Completed" && t.status != "Cancelled")
                .ToListAsync();
        }
    }
}