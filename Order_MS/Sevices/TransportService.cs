using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;

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
                return (false, $"Order request #{dto.OrderReqId} not found.", null);

            if (orderRequest.ReqStatus != "Approved")
                return (false,
                    $"Order request must be 'Approved' before transport can be assigned. " +
                    $"Current status: '{orderRequest.ReqStatus}'.", null);

            if (dto.Assignments is null || !dto.Assignments.Any())
                return (false, "At least one driver-vehicle assignment is required.", null);

            
            int requiredQty = orderRequest.TotalQuantity ?? 0;
            int assignedQty = dto.Assignments.Sum(a => a.Quantity);

            if (assignedQty < requiredQty)
                return (false,
                    $"Assigned quantity ({assignedQty}) is less than " +
                    $"the order's total quantity ({requiredQty}). " +
                    "Add more vehicles or increase per-vehicle quantities.", null);

            
            var availableLinks = (await _repo.GetAvailableDriverVehicleLinksAsync())
                                  .ToDictionary(l => l.ConnectionId);

            foreach (var item in dto.Assignments)
            {
                if (!availableLinks.TryGetValue(item.ConnectionId, out var link))
                    return (false,
                        $"Driver-vehicle link #{item.ConnectionId} is not available " +
                        "or does not exist. Refresh the available-links list and try again.", null);

                
                if (link.Vehicle?.Capacity.HasValue == true &&
                    item.Quantity > link.Vehicle.Capacity.Value)
                    return (false,
                        $"Vehicle '{link.Vehicle.VehicleNumber}' has capacity " +
                        $"{link.Vehicle.Capacity} but was assigned {item.Quantity} units.", null);
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

            await _repo.SaveAsync();

  
            var saved = await _repo.GetAssignmentsByOrderRequestAsync(dto.OrderReqId);
            return (true, "Transport assigned successfully.", saved.Select(ToAssignmentDto));
        }

 

        public async Task<(bool Success, string Message)>
            UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status value cannot be empty.");

            var updated = await _repo.UpdateAssignmentStatusAsync(assignmentId, dto.Status);
            if (!updated) return (false, $"Assignment #{assignmentId} not found.");

            await _repo.SaveAsync();
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