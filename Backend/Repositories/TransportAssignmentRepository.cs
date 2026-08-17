using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.Models;

namespace Order_MS.Repositories
{
    public class TransportAssignmentRepository : ITransportAssignmentRepository
    {
        private readonly OrderMSDbContext _context;

        public TransportAssignmentRepository(OrderMSDbContext context)
        {
            _context = context;
        }

        // order requests

        public async Task<IEnumerable<OrderRequest>> GetApprovedOrderRequestsAsync()
        {
            return await _context.OrderRequests
                .Include(or => or.Branch)
                .Include(or => or.OrderRequestLines)
                    .ThenInclude(l => l.Item)
                .Where(or => or.ReqStatus == "Approved")
                .OrderByDescending(or => or.RequestedOn)
                .ToListAsync();
        }

        public async Task<OrderRequest?> GetOrderRequestWithDetailsAsync(int orderReqId)
        {
            return await _context.OrderRequests
                .Include(or => or.Branch)
                .Include(or => or.OrderRequestLines)
                    .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(or => or.OrderReqId == orderReqId);
        }

        public async Task<bool> UpdateOrderRequestStatusAsync(int orderReqId, string newStatus)
        {
            var orderRequest = await _context.OrderRequests.FindAsync(orderReqId);
            if (orderRequest is null) return false;

            orderRequest.ReqStatus = newStatus;
            orderRequest.ModifiedOn = DateTime.UtcNow;
            return true;
        }

        // Driver Vehicle Links

        public async Task<IEnumerable<DriverVehicleLink>> GetAvailableDriverVehicleLinksAsync()
        {
            return await _context.DriverVehicleLinks
                .Include(dvl => dvl.Driver)
                .Include(dvl => dvl.Vehicle)
                .Where(dvl =>
                    dvl.Driver.Available == "Available" &&
                    dvl.Vehicle.Available == "Available")
                .ToListAsync();
        }

        public async Task<IEnumerable<DriverVehicleLink>> GetAllDriverVehicleLinksAsync()
        {
            return await _context.DriverVehicleLinks
                .Include(dvl => dvl.Driver)
                .Include(dvl => dvl.Vehicle)
                .ToListAsync();
        }

        public async Task AddDriverVehicleLinkAsync(DriverVehicleLink link)
        {
            await _context.DriverVehicleLinks.AddAsync(link);
        }

        public async Task<bool> DeleteDriverVehicleLinkAsync(int connectionId)
        {
            var link = await _context.DriverVehicleLinks.FindAsync(connectionId);
            if (link is null) return false;

            _context.DriverVehicleLinks.Remove(link);
            return true;
        }

        public async Task<bool> HasAssignmentsForLinkAsync(int connectionId)
        {
            var inTransport = await _context.TransportAssignments.AnyAsync(ta => ta.ConnectionId == connectionId);
            var inOrders = await _context.Orders.AnyAsync(o => o.ConnectionId == connectionId);
            return inTransport || inOrders;
        }


        public async Task<IEnumerable<TransportAssignment>> GetAllAssignmentsAsync()
        {
            return await _context.TransportAssignments
   
                .Include(ta => ta.OrderReq)
                    .ThenInclude(or => or!.Branch)
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Driver)
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Vehicle)
                .OrderByDescending(ta => ta.AssignedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<TransportAssignment>> GetAssignmentsByOrderRequestAsync(int orderReqId)
        {
            return await _context.TransportAssignments
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Driver)
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Vehicle)
                .Where(ta => ta.OrderReqId == orderReqId)
                .ToListAsync();
        }

        public async Task<TransportAssignment?> GetAssignmentWithDetailsAsync(int assignmentId)
        {
            return await _context.TransportAssignments
                .Include(ta => ta.OrderReq)
                    .ThenInclude(or => or!.Branch)
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Driver)
                .Include(ta => ta.Connection)
                    .ThenInclude(dvl => dvl!.Vehicle)
                .FirstOrDefaultAsync(ta => ta.AssignmentId == assignmentId);
        }

        public async Task AddAssignmentAsync(TransportAssignment assignment)
        {
            await _context.TransportAssignments.AddAsync(assignment);
        }

        public async Task<bool> UpdateAssignmentStatusAsync(int assignmentId, string newStatus)
        {
            var assignment = await _context.TransportAssignments.FindAsync(assignmentId);
            if (assignment is null) return false;

            assignment.Status = newStatus;
            return true;
        }

        // Driver and Vehicle

        public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            return await _context.Vehicles.FindAsync(vehicleId);
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
        }

        public async Task<IEnumerable<Driver>> GetDriversAsync()
        {
            return await _context.Drivers.ToListAsync();
        }

        public async Task<Driver?> GetDriverByIdAsync(int driverId)
        {
            return await _context.Drivers.FindAsync(driverId);
        }

        public async Task AddDriverAsync(Driver driver)
        {
            await _context.Drivers.AddAsync(driver);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}