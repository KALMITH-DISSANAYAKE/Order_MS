using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;

namespace Order_MS.Services
{
    public class TransportService : ITransportService
    {
        private readonly OrderMSDbContext _context;

        public TransportService(OrderMSDbContext context)
        {
            _context = context;
        }

        public async Task<TransportResponseDto> AssignTransportAsync(AssignTransportDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) throw new Exception("Order not found");
            if (order.order_status != "Paid") throw new Exception("Payment not completed");

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null || !vehicle.Available) throw new Exception("Vehicle not available");

            var driver = await _context.Drivers.FindAsync(dto.DriverId);
            if (driver == null || !driver.Available) throw new Exception("Driver not available");

            var link = await _context.DriverVehicleLinks
                .FirstOrDefaultAsync(l => l.driver_id == dto.DriverId && l.vehicle_id == dto.VehicleId);
            if (link == null) throw new Exception("Driver not linked to this vehicle");

            var assignment = new TransportAssignment
            {
                order_id = dto.OrderId,
                vehicle_id = dto.VehicleId,
                driver_id = dto.DriverId,
                assigned_on = DateTime.UtcNow,
                status = "Assigned"
            };

            order.order_status = "ReadyForDelivery";
            order.modified_on = DateTime.UtcNow;
            vehicle.Available = false;
            driver.Available = false;

            _context.TransportAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return new TransportResponseDto
            {
                AssignmentId = assignment.assignment_id,
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleId = vehicle.vehicle_id,
                VehicleNumber = vehicle.vehicle_number,
                DriverId = driver.driver_id,
                DriverLicense = driver.license_number,
                AssignedOn = assignment.assigned_on,
                Status = assignment.status
            };
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .Where(v => v.Available)
                .Select(v => new VehicleDto
                {
                    VehicleId = v.vehicle_id,
                    VehicleNumber = v.vehicle_number,
                    Available = v.Available
                }).ToListAsync();
        }

        public async Task<List<DriverDto>> GetAvailableDriversAsync()
        {
            return await _context.Drivers
                .Where(d => d.Available)
                .Select(d => new DriverDto
                {
                    DriverId = d.driver_id,
                    LicenseNumber = d.license_number,
                    Available = d.Available
                }).ToListAsync();
        }

        public async Task<List<TransportResponseDto>> GetAssignmentsAsync()
        {
            return await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Select(t => new TransportResponseDto
                {
                    AssignmentId = t.assignment_id,
                    OrderId = t.order_id,
                    OrderStatus = t.Order.order_status,
                    VehicleId = t.vehicle_id,
                    VehicleNumber = t.Vehicle.vehicle_number,
                    DriverId = t.driver_id,
                    DriverLicense = t.Driver.license_number,
                    AssignedOn = t.assigned_on,
                    Status = t.status
                }).ToListAsync();
        }

        public async Task<TransportResponseDto> GetAssignmentByOrderIdAsync(int orderId)
        {
            var t = await _context.TransportAssignments
                .Include(t => t.Order)
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.order_id == orderId);

            if (t == null) throw new Exception("Assignment not found");

            return new TransportResponseDto
            {
                AssignmentId = t.assignment_id,
                OrderId = t.order_id,
                OrderStatus = t.Order.order_status,
                VehicleId = t.vehicle_id,
                VehicleNumber = t.Vehicle.vehicle_number,
                DriverId = t.driver_id,
                DriverLicense = t.Driver.license_number,
                AssignedOn = t.assigned_on,
                Status = t.status
            };
        }
    }
}