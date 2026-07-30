using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;
using Order_MS.Exceptions;  

namespace Order_MS.Services
{
    public class TransportService : ITransportService
    {
        private readonly ITransportAssignmentRepository _repo;

        public TransportService(ITransportAssignmentRepository repo)
        {
            _repo = repo;
        }


        public async Task<IEnumerable<OrderRequestForTransportDto>> GetApprovedOrderRequestsAsync()
        {
            var list = await _repo.GetApprovedOrderRequestsAsync();
            return list.Select(ToOrderRequestDto);
        }

        public async Task<IEnumerable<AvailableDriverVehicleLinkDto>> GetAvailableDriverVehicleLinksAsync()
        {
            var list = await _repo.GetAvailableDriverVehicleLinksAsync();
            return list.Select(ToAvailableLinkDto);
        }

        public async Task<IEnumerable<TransportAssignmentResponseDto>> GetAllAssignmentsAsync()
        {
            var list = await _repo.GetAllAssignmentsAsync();
            return list.Select(ToAssignmentDto);
        }

        public async Task<IEnumerable<TransportAssignmentResponseDto>> GetAssignmentsByOrderRequestAsync(int orderReqId)
        {
            var list = await _repo.GetAssignmentsByOrderRequestAsync(orderReqId);
            return list.Select(ToAssignmentDto);
        }

       

        public async Task<(bool Success, string Message, IEnumerable<TransportAssignmentResponseDto>? Data)>
            AssignTransportAsync(AssignTransportDto dto)
        {
            
            var orderRequest = await _repo.GetOrderRequestWithDetailsAsync(dto.OrderReqId);
            if (orderRequest is null)
                throw new BusinessException($"Order request #{dto.OrderReqId} not found.", 404);

            if (orderRequest.ReqStatus != "Approved")
                throw new BusinessException($"Order request must be 'Approved' before transport can be assigned. Current status: '{orderRequest.ReqStatus}'.", 400);

            if (dto.Assignments is null || !dto.Assignments.Any())
                throw new BusinessException("At least one driver-vehicle assignment is required.", 400);

            
            int requiredQty = orderRequest.TotalQuantity ?? 0;
            int assignedQty = dto.Assignments.Sum(a => a.Quantity);

            if (assignedQty < requiredQty)
                  throw new BusinessException($"Assigned quantity ({assignedQty}) is less than the order's total quantity ({requiredQty}). Add more vehicles or increase per-vehicle quantities.", 400);

            
            var availableLinks = (await _repo.GetAvailableDriverVehicleLinksAsync())
                                  .ToDictionary(l => l.ConnectionId);

            foreach (var item in dto.Assignments)
            {
                if (!availableLinks.TryGetValue(item.ConnectionId, out var link))
                    throw new BusinessException($"Driver-vehicle link #{item.ConnectionId} is not available or does not exist. Refresh the available-links list and try again.", 404);
                
                if (link.Vehicle?.Capacity.HasValue == true && item.Quantity > link.Vehicle.Capacity.Value)
                    throw new BusinessException($"Vehicle '{link.Vehicle.VehicleNumber}' has capacity {link.Vehicle.Capacity} but was assigned {item.Quantity} units.", 400);   
            }
             
     
            foreach (var item in dto.Assignments)
            {
                await _repo.AddAssignmentAsync(new TransportAssignment
                {
                    OrderReqId = dto.OrderReqId,
                    ConnectionId = item.ConnectionId,
                    AssignedOn = DateTime.UtcNow,
                    Status = "Assigned",     
                    Quantity = item.Quantity
                });
            }

            await _repo.UpdateOrderRequestStatusAsync(dto.OrderReqId, "TransportAssigned");

            try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException($"Database error: {ex.InnerException?.Message ?? ex.Message}", 400);
    }
  
            var saved = await _repo.GetAssignmentsByOrderRequestAsync(dto.OrderReqId);
            return (true, "Transport assigned successfully.", saved.Select(ToAssignmentDto));
        }

 

        public async Task<(bool Success, string Message)>
            UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
                throw new BusinessException("Status value cannot be empty.", 400);
                
            var updated = await _repo.UpdateAssignmentStatusAsync(assignmentId, dto.Status);
            if (!updated)
                throw new BusinessException($"Assignment #{assignmentId} not found.", 404);

           try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException($"Database error: {ex.InnerException?.Message ?? ex.Message}", 400);
            }
            return (true, $"Assignment #{assignmentId} status updated to '{dto.Status}'."); 
        }


        private static OrderRequestForTransportDto ToOrderRequestDto(OrderRequest or) => new()
        {
            OrderReqId = or.OrderReqId,
            ReqStatus = or.ReqStatus ?? string.Empty,
            TotalQuantity = or.TotalQuantity,
            TotalPrice = or.TotalPrice,
            BranchId = or.BranchId,
            BranchLocation = or.Branch?.Location ?? string.Empty,
            RequestedOn = or.RequestedOn ?? DateTime.MinValue,
            Lines = (or.OrderRequestLines ?? Enumerable.Empty<OrderRequestLine>())
                        .Select(l => new OrderRequestLineForTransportDto
                        {
                            ItemId = l.ItemId,
                            ItemName = l.Item?.ItemName ?? string.Empty,
                            Quantity = l.Quantity
                        }).ToList()
        };

        private static AvailableDriverVehicleLinkDto ToAvailableLinkDto(DriverVehicleLink dvl) => new()
        {
            ConnectionId = dvl.ConnectionId,
            DriverId = dvl.DriverId,
            DriverName = dvl.Driver?.DriversName ?? string.Empty,
            LicenseNumber = dvl.Driver?.LicenseNumber ?? string.Empty,
            VehicleId = dvl.VehicleId,
            VehicleNumber = dvl.Vehicle?.VehicleNumber ?? string.Empty,
            Capacity = dvl.Vehicle?.Capacity
        };

        private static TransportAssignmentResponseDto ToAssignmentDto(TransportAssignment ta) => new()
        {
            AssignmentId = ta.AssignmentId,
            OrderReqId = ta.OrderReqId,
            ConnectionId = ta.ConnectionId,
         
            DriverName = ta.Connection?.Driver?.DriversName ?? string.Empty,
            LicenseNumber = ta.Connection?.Driver?.LicenseNumber ?? string.Empty,
            VehicleNumber = ta.Connection?.Vehicle?.VehicleNumber ?? string.Empty,
            VehicleCapacity = ta.Connection?.Vehicle?.Capacity,
            AssignedOn = ta.AssignedOn,
            Status = ta.Status,
            Quantity = ta.Quantity
        };
    }
}