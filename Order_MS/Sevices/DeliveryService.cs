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

            var assignment = await _assignmentRepo.GetByOrderIdWithDetailsAsync(order.order_id);
            if (assignment == null) throw new Exception("No transport assignment found");
            if (assignment.Vehicle.vehical_number != dto.VehicleNumber)
                throw new Exception("Vehicle number does not match assignment");

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleNumber = assignment.Vehicle.vehical_number,
                DriverLicense = assignment.Driver.license_number
            };
        }

        public async Task<DeliveryResponseDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto)
        {
            var order = await _orderRepo.GetByIdAsync((object)dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            var assignment = await _assignmentRepo.GetByOrderIdWithDetailsAsync(dto.OrderId);

            order.order_status = dto.Status;
            order.modified_on = DateTime.UtcNow;
            _orderRepo.Update(order);

            if (dto.Status == "Delivered" && assignment != null)
            {
                assignment.status = "Delivered";
                _assignmentRepo.Update(assignment);

                if (assignment.Vehicle != null)
                {
                    assignment.Vehicle.Available = "Available";
                    _vehicleRepo.Update(assignment.Vehicle);
                }
                if (assignment.Driver != null)
                {
                    assignment.Driver.Available = "Available";
                    _driverRepo.Update(assignment.Driver);
                }
            }

            await _orderRepo.SaveAsync();

            return new DeliveryResponseDto
            {
                OrderId = order.order_id,
                OrderStatus = order.order_status,
                VehicleNumber = assignment?.Vehicle?.vehical_number,
                DriverLicense = assignment?.Driver?.license_number,
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
            if (order.order_status != "Delivered") throw new Exception("Order not delivered yet");

            order.order_status = "Completed";
            order.modified_on = DateTime.UtcNow;
            _orderRepo.Update(order);

            var assignment = await _assignmentRepo.GetByOrderIdAsync(orderId);
            if (assignment != null)
            {
                assignment.status = "Completed";
                _assignmentRepo.Update(assignment);
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