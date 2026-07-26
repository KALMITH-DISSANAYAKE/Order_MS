using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;

namespace Order_MS.Services
{
    public class TransportService : ITransportService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IGenericRepository<Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Driver> _driverRepo;
        private readonly IGenericRepository<DriverVehicleLink> _linkRepo;
        private readonly ITransportAssignmentRepository _assignmentRepo;

        public TransportService(
            IOrderRepository orderRepo,
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

            // Allow assignment on Pending/Approved/TransportAssigned orders
            if (order.order_status != "Pending" && order.order_status != "Approved" && order.order_status != "TransportAssigned")
                throw new Exception("Order not available for transport assignment");

            // 1. Order quantity check
            var orderTotalQty = await _orderRepo.GetOrderTotalQuantityAsync(dto.OrderId);
            var orderAssignments = await _assignmentRepo.GetAllByOrderIdAsync(dto.OrderId);
            var alreadyAssignedQty = orderAssignments.Sum(a => a.quantity ?? 0);
            var remainingOrderQty = orderTotalQty - alreadyAssignedQty;

            if (remainingOrderQty <= 0)
                throw new Exception("Order fully assigned already");

            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            if (dto.Quantity > remainingOrderQty)
                throw new Exception($"Only {remainingOrderQty} quantity remaining to assign for this order");

            // 2. Vehicle capacity check
            var vehicle = await _vehicleRepo.GetByIdAsync((object)dto.VehicleId);
            if (vehicle == null) throw new Exception("Vehicle not found");

            var vehicleCapacity = vehicle.capacity ?? 0;
            var vehicleActiveAssignments = await _assignmentRepo.GetActiveByVehicleIdAsync(dto.VehicleId);
            var vehicleUsedCapacity = vehicleActiveAssignments.Sum(a => a.quantity ?? 0);
            var vehicleRemaining = vehicleCapacity - vehicleUsedCapacity;

            if (dto.Quantity > vehicleRemaining)
                throw new Exception($"Vehicle only has {vehicleRemaining} remaining capacity");

            // 3. Driver check
            var driver = await _driverRepo.GetByIdAsync((object)dto.DriverId);
            if (driver == null) throw new Exception("Driver not found");

            // Driver can be used if Available OR already assigned to this same vehicle
            var driverActiveAssignments = await _assignmentRepo.GetActiveByDriverIdAsync(dto.DriverId);
            bool driverOnThisVehicle = driverActiveAssignments.Any(a => a.vehicle_id == dto.VehicleId);

            if (driver.Available != "Available" && !driverOnThisVehicle)
                throw new Exception("Driver already assigned to another vehicle");

            // 4. Driver-Vehicle link check
            var allLinks = await _linkRepo.GetAllAsync();
            var link = allLinks.FirstOrDefault(l => l.driver_id == dto.DriverId && l.vehical_id == dto.VehicleId);
            if (link == null) throw new Exception("Driver not linked to this vehicle");

            // 5. Create assignment
            var assignment = new TransportAssignment
            {
                order_id = dto.OrderId,
                vehicle_id = dto.VehicleId,
                driver_id = dto.DriverId,
                quantity = dto.Quantity,
                assigned_on = DateTime.UtcNow,
                status = "Assigned"
            };

            // Update order status on first assignment
            if (!orderAssignments.Any())
            {
                order.order_status = "TransportAssigned";
                order.modified_on = DateTime.UtcNow;
                _orderRepo.Update(order);
            }

            // Mark vehicle unavailable if fully loaded
            if (dto.Quantity >= vehicleRemaining)
            {
                vehicle.Available = "Not Available";
                _vehicleRepo.Update(vehicle);
            }

            // Mark driver unavailable if newly assigned
            if (driver.Available == "Available")
            {
                driver.Available = "Not Available";
                _driverRepo.Update(driver);
            }

            await _assignmentRepo.AddAsync(assignment);
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
                Quantity = assignment.quantity ?? 0,
                AssignedOn = assignment.assigned_on,
                Status = assignment.status
            };
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync()
        {
            var allVehicles = await _vehicleRepo.GetAllAsync();
            var allAssignments = await _assignmentRepo.GetAllAsync();

            var result = new List<VehicleDto>();
            foreach (var v in allVehicles)
            {
                var used = allAssignments
                    .Where(a => a.vehicle_id == v.vehical_id && a.status != "Delivered" && a.status != "Completed" && a.status != "Cancelled")
                    .Sum(a => a.quantity ?? 0);

                var remaining = (v.capacity ?? 0) - used;

                if (remaining > 0)
                {
                    result.Add(new VehicleDto
                    {
                        VehicleId = v.vehical_id,
                        VehicleNumber = v.vehical_number,
                        Available = v.Available,
                        Capacity = v.capacity ?? 0,
                        RemainingCapacity = remaining
                    });
                }
            }
            return result;
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
                Quantity = t.quantity ?? 0,
                AssignedOn = t.assigned_on,
                Status = t.status
            }).ToList();
        }

        public async Task<List<TransportResponseDto>> GetAssignmentsByOrderIdAsync(int orderId)
        {
            var assignments = await _assignmentRepo.GetAllByOrderIdWithDetailsAsync(orderId);
            return assignments.Select(t => new TransportResponseDto
            {
                AssignmentId = t.assignment_id,
                OrderId = t.order_id,
                OrderStatus = t.Order.order_status,
                VehicleId = t.vehicle_id,
                VehicleNumber = t.Vehicle.vehical_number,
                DriverId = t.driver_id,
                DriverLicense = t.Driver.license_number,
                Quantity = t.quantity ?? 0,
                AssignedOn = t.assigned_on,
                Status = t.status
            }).ToList();
        }
    }
}