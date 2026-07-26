using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;

namespace Order_MS.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITransportAssignmentRepository _assignmentRepo;
        private readonly IGenericRepository<Vehicle> _vehicleRepo;
        private readonly IGenericRepository<Driver> _driverRepo;

        public DeliveryService(
            IOrderRepository orderRepo,
            ITransportAssignmentRepository assignmentRepo,
            IGenericRepository<Vehicle> vehicleRepo,
            IGenericRepository<Driver> driverRepo)
        {
            _orderRepo = orderRepo;
            _assignmentRepo = assignmentRepo;
            _vehicleRepo = vehicleRepo;
            _driverRepo = driverRepo;
        }

        public async Task<DeliveryResponseDto> VerifyDeliveryAsync(VerifyDeliveryDto dto)
        {
            var order = await _orderRepo.GetByOrderNumberAsync(dto.OrderNumber);
            if (order == null) throw new Exception("Order not found");

            var assignments = await _assignmentRepo.GetAllByOrderIdWithDetailsAsync(order.order_id);
            if (!assignments.Any()) throw new Exception("No transport assignment found");

            var matchingAssignment = assignments.FirstOrDefault(a => a.Vehicle.vehical_number == dto.VehicleNumber);
            if (matchingAssignment == null)
                throw new Exception("Vehicle number does not match any assignment for this order");

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleNumber = matchingAssignment.Vehicle.vehical_number,
                DriverLicense = matchingAssignment.Driver.license_number
            };
        }

        public async Task<DeliveryResponseDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto)
        {
            var order = await _orderRepo.GetByIdAsync((object)dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            // For delivery updates, order should be Paid or InTransit
            if (order.order_status != "Paid" && order.order_status != "InTransit" && order.order_status != "TransportAssigned")
                throw new Exception("Order not ready for delivery");

            var assignments = await _assignmentRepo.GetAllByOrderIdAsync(dto.OrderId);
            var assignment = assignments.FirstOrDefault();
            if (assignment == null) throw new Exception("No assignment found");

            order.order_status = dto.Status;
            order.modified_on = DateTime.UtcNow;
            _orderRepo.Update(order);

            foreach (var a in assignments)
            {
                if (dto.Status == "InTransit" && a.status == "Assigned")
                {
                    a.status = "InTransit";
                    _assignmentRepo.Update(a);
                }
                else if (dto.Status == "Delivered" && (a.status == "Assigned" || a.status == "InTransit"))
                {
                    a.status = "Delivered";
                    _assignmentRepo.Update(a);

                    // Free vehicle if no other active assignments
                    var vehicleActive = await _assignmentRepo.GetActiveByVehicleIdAsync(a.vehicle_id);
                    if (!vehicleActive.Any())
                    {
                        var vehicle = await _vehicleRepo.GetByIdAsync((object)a.vehicle_id);
                        if (vehicle != null)
                        {
                            vehicle.Available = "Available";
                            _vehicleRepo.Update(vehicle);
                        }
                    }

                    // Free driver if no other active assignments
                    var driverActive = await _assignmentRepo.GetActiveByDriverIdAsync(a.driver_id);
                    if (!driverActive.Any())
                    {
                        var driver = await _driverRepo.GetByIdAsync((object)a.driver_id);
                        if (driver != null)
                        {
                            driver.Available = "Available";
                            _driverRepo.Update(driver);
                        }
                    }
                }
            }

            await _orderRepo.SaveAsync();

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                DeliveredOn = dto.Status == "Delivered" ? DateTime.UtcNow : null
            };
        }

        public async Task<List<DeliveryHistoryDto>> GetDeliveryHistoryAsync()
        {
            var assignments = await _assignmentRepo.GetDeliveredOrInTransitAsync();
            return assignments.Select(t => new DeliveryHistoryDto
            {
                OrderId = t.order_id,
                OrderStatus = t.Order.order_status,
                VehicleNumber = t.Vehicle.vehical_number,
                DriverLicense = t.Driver.license_number,
                DeliveredOn = t.status == "Delivered" ? t.assigned_on : null
            }).ToList();
        }

        public async Task<DeliveryResponseDto> ConfirmDeliveryAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync((object)orderId);
            if (order == null) throw new Exception("Order not found");

            var assignments = await _assignmentRepo.GetAllByOrderIdAsync(orderId);
            if (assignments.Any(a => a.status != "Delivered"))
                throw new Exception("Not all deliveries completed yet");

            order.order_status = "Completed";
            order.modified_on = DateTime.UtcNow;
            _orderRepo.Update(order);

            foreach (var a in assignments)
            {
                a.status = "Completed";
                _assignmentRepo.Update(a);
            }

            await _orderRepo.SaveAsync();

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status
            };
        }
    }
}