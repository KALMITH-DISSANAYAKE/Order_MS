using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;

namespace Order_MS.Services
{
    public class TransportService : ITransportService
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Driver> _driverRepo;
        private readonly IGenericRepository<DriverVehicleLink> _linkRepo;
        private readonly ITransportAssignmentRepository _assignmentRepo;

        public TransportService(
            IGenericRepository<Order> orderRepo,
            IGenericRepository<Vehicle> vehicleRepo,
            IGenericRepository<Driver> driverRepo,
            IGenericRepository<DriverVehicleLink> linkRepo,
            ITransportAssignmentRepository assignmentRepo)
        {
            _orderRepo = orderRepo;
            _vehicleRepo = vehicleRepo;
            _driverRepo = driverRepo;
            _linkRepo = linkRepo;
            _assignmentRepo = assignmentRepo;
        }

        public async Task<TransportResponseDto> AssignTransportAsync(AssignTransportDto dto)
        {
            var order = await _orderRepo.GetByIdAsync((object)dto.OrderId);
            if (order == null) throw new Exception("Order not found");
            if (order.order_status != "Paid") throw new Exception("Payment not completed");

            var vehicle = await _vehicleRepo.GetByIdAsync((object)dto.VehicleId);
            if (vehicle == null || vehicle.Available != "Available") throw new Exception("Vehicle not available");

            var driver = await _driverRepo.GetByIdAsync((object)dto.DriverId);
            if (driver == null || driver.Available != "Available") throw new Exception("Driver not available");

            var allLinks = await _linkRepo.GetAllAsync();
            var link = allLinks.FirstOrDefault(l => l.driver_id == dto.DriverId && l.vehical_id == dto.VehicleId);
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
            vehicle.Available = "Not Available";
            driver.Available = "Not Available";

            await _assignmentRepo.AddAsync(assignment);
            _orderRepo.Update(order);
            _vehicleRepo.Update(vehicle);
            _driverRepo.Update(driver);

            await _orderRepo.SaveAsync();

            return new TransportResponseDto
            {
                AssignmentId = assignment.assignment_id,
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleId = vehicle.vehical_id,
                VehicleNumber = vehicle.vehical_number,
                DriverId = driver.driver_id,
                DriverLicense = driver.license_number,
                AssignedOn = assignment.assigned_on,
                Status = assignment.status
            };
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync()
        {
            var all = await _vehicleRepo.GetAllAsync();
            return all.Where(v => v.Available == "Available")
                .Select(v => new VehicleDto
                {
                    VehicleId = v.vehical_id,
                    VehicleNumber = v.vehical_number,
                    Available = v.Available
                }).ToList();
        }

        public async Task<List<DriverDto>> GetAvailableDriversAsync()
        {
            var all = await _driverRepo.GetAllAsync();
            return all.Where(d => d.Available == "Available")
                .Select(d => new DriverDto
                {
                    DriverId = d.driver_id,
                    LicenseNumber = d.license_number,
                    Available = d.Available
                }).ToList();
        }

        public async Task<List<TransportResponseDto>> GetAssignmentsAsync()
        {
            var assignments = await _assignmentRepo.GetAllWithDetailsAsync();
            return assignments.Select(t => new TransportResponseDto
            {
                AssignmentId = t.assignment_id,
                OrderId = t.order_id,
                OrderStatus = t.Order.order_status,
                VehicleId = t.vehicle_id,
                VehicleNumber = t.Vehicle.vehical_number,
                DriverId = t.driver_id,
                DriverLicense = t.Driver.license_number,
                AssignedOn = t.assigned_on,
                Status = t.status
            }).ToList();
        }

        public async Task<TransportResponseDto> GetAssignmentByOrderIdAsync(int orderId)
        {
            var t = await _assignmentRepo.GetByOrderIdWithDetailsAsync(orderId);
            if (t == null) throw new Exception("Assignment not found");

            return new TransportResponseDto
            {
                AssignmentId = t.assignment_id,
                OrderId = t.order_id,
                OrderStatus = t.Order.order_status,
                VehicleId = t.vehicle_id,
                VehicleNumber = t.Vehicle.vehical_number,
                DriverId = t.driver_id,
                DriverLicense = t.Driver.license_number,
                AssignedOn = t.assigned_on,
                Status = t.status
            };
        }
    }
}