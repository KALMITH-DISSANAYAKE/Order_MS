using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;

namespace Order_MS.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly OrderMSDbContext _context;

        public DeliveryService(OrderMSDbContext context)
        {
            _context = context;
        }

        public async Task<DeliveryResponseDto> VerifyDeliveryAsync(VerifyDeliveryDto dto)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.order_id.ToString() == dto.OrderNumber);

            if (order == null) throw new Exception("Order not found");

            var assignment = await _context.TransportAssignments
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.order_id == order.order_id);

            if (assignment == null) throw new Exception("No transport assignment found");
            if (assignment.Vehicle.vehicle_number != dto.VehicleNumber)
                throw new Exception("Vehicle number does not match assignment");

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleNumber = assignment.Vehicle.vehicle_number,
                DriverLicense = assignment.Driver.license_number
            };
        }

        public async Task<DeliveryResponseDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            var assignment = await _context.TransportAssignments
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.order_id == dto.OrderId);

            order.order_status = dto.Status;
            order.modified_on = DateTime.UtcNow;

            if (dto.Status == "Delivered" && assignment != null)
            {
                assignment.status = "Delivered";
                if (assignment.Vehicle != null) assignment.Vehicle.Available = true;
                if (assignment.Driver != null) assignment.Driver.Available = true;
            }

            await _context.SaveChangesAsync();

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleNumber = assignment?.Vehicle?.vehicle_number,
                DriverLicense = assignment?.Driver?.license_number,
                DeliveredOn = dto.Status == "Delivered" ? DateTime.UtcNow : null
            };
        }

        public async Task<List<DeliveryHistoryDto>> GetDeliveryHistoryAsync(int? branchId = null)
        {
            var query = _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.status == "Delivered" || t.status == "Completed" || t.Order.order_status == "InTransit")
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(t => t.Order.branch_id == branchId.Value);

            return await query.Select(t => new DeliveryHistoryDto
            {
                OrderId = t.order_id,
                BranchId = t.Order.branch_id,
                OrderStatus = t.Order.order_status,
                VehicleNumber = t.Vehicle.vehicle_number,
                DriverLicense = t.Driver.license_number,
                DeliveredOn = t.status == "Delivered" ? t.assigned_on : null
            }).ToListAsync();
        }

        public async Task<DeliveryResponseDto> ConfirmDeliveryAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Order not found");
            if (order.order_status != "Delivered") throw new Exception("Order not delivered yet");

            order.order_status = "Completed";
            order.modified_on = DateTime.UtcNow;

            var assignment = await _context.TransportAssignments
                .FirstOrDefaultAsync(t => t.order_id == orderId);
            if (assignment != null) assignment.status = "Completed";

            await _context.SaveChangesAsync();

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status
            };
        }
    }
}