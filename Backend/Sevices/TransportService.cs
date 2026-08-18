using Microsoft.EntityFrameworkCore;
using Order_MS.DTOs;
using Order_MS.Exceptions;  
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

        public async Task<IEnumerable<AvailableDriverVehicleLinkDto>> GetAllDriverVehicleLinksAsync()
        {
            var list = await _repo.GetAllDriverVehicleLinksAsync();
            return list.Select(ToAvailableLinkDto);
        }

        public async Task<(bool Success, string Message, AvailableDriverVehicleLinkDto? Data)> CreateDriverVehicleLinkAsync(CreateDriverVehicleLinkDto dto)
        {
            var link = new DriverVehicleLink
            {
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                CreatedOn = DateTime.UtcNow,
                Status = "Available"
            };

            await _repo.AddDriverVehicleLinkAsync(link);

            var driver = await _repo.GetDriverByIdAsync(dto.DriverId);
            if (driver != null) driver.Available = "Assigned";

            var vehicle = await _repo.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle != null) vehicle.Available = "Assigned";
            
            try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.InnerException?.Message ?? ex.Message}", null);
            }

            // Fetch again to get related Driver and Vehicle data for the DTO
            var allLinks = await _repo.GetAllDriverVehicleLinksAsync();
            var savedLink = allLinks.FirstOrDefault(l => l.ConnectionId == link.ConnectionId);

            return (true, "Driver-vehicle link created successfully.", savedLink != null ? ToAvailableLinkDto(savedLink) : null);
        }

        public async Task<(bool Success, string Message)> DeleteDriverVehicleLinkAsync(int connectionId)
        {
            var isAssigned = await _repo.HasAssignmentsForLinkAsync(connectionId);
            if (isAssigned)
            {
                return (false, "Cannot delete because this link is assigned to an order request.");
            }

            var allLinks = await _repo.GetAllDriverVehicleLinksAsync();
            var link = allLinks.FirstOrDefault(l => l.ConnectionId == connectionId);

            if (link != null)
            {
                var driver = await _repo.GetDriverByIdAsync(link.DriverId);
                if (driver != null) driver.Available = "Available";

                var vehicle = await _repo.GetVehicleByIdAsync(link.VehicleId);
                if (vehicle != null) vehicle.Available = "Available";
            }

            var deleted = await _repo.DeleteDriverVehicleLinkAsync(connectionId);
            if (!deleted)
                return (false, $"Driver-vehicle link #{connectionId} not found.");

            try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }

            return (true, $"Driver-vehicle link #{connectionId} deleted successfully.");
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
            AssignTransportAsync(AssignTransportDto dto, int? userId = null)
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
                if (availableLinks.TryGetValue(item.ConnectionId, out var link))
                {
                    link.Status = "Assigned";
                }
                
                await _repo.AddAssignmentAsync(new TransportAssignment
                {
                    OrderReqId = dto.OrderReqId,
                    ConnectionId = item.ConnectionId,
                    AssignedOn = DateTime.UtcNow,
                    Status = "Assigned",     
                    Quantity = item.Quantity
                });
            }

            await _repo.UpdateOrderRequestStatusAsync(dto.OrderReqId, "TransportAssigned", userId);

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

        // Vehicles

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _repo.GetVehiclesAsync();
            return vehicles.Select(v => new VehicleDto
            {
                VehicleId = v.VehicleId,
                VehicleNumber = v.VehicleNumber,
                Capacity = v.Capacity,
                Available = v.Available ?? "Available"
            });
        }

        public async Task<(bool Success, string Message, VehicleDto? Data)> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var vehicle = new Vehicle
            {
                VehicleNumber = dto.VehicleNumber,
                Capacity = dto.Capacity,
                Available = dto.Available
            };

            await _repo.AddVehicleAsync(vehicle);
            await _repo.SaveAsync();

            var result = new VehicleDto
            {
                VehicleId = vehicle.VehicleId,
                VehicleNumber = vehicle.VehicleNumber,
                Capacity = vehicle.Capacity,
                Available = vehicle.Available ?? "Available"
            };

            return (true, "Vehicle added successfully.", result);
        }

        public async Task<(bool Success, string Message, VehicleDto? Data)> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto)
        {
            var vehicle = await _repo.GetVehicleByIdAsync(vehicleId);
            if (vehicle is null)
            {
                return (false, "Vehicle not found.", null);
            }

            vehicle.VehicleNumber = dto.VehicleNumber;
            vehicle.Capacity = dto.Capacity;
            vehicle.Available = dto.Available;

            await _repo.SaveAsync();

            var result = new VehicleDto
            {
                VehicleId = vehicle.VehicleId,
                VehicleNumber = vehicle.VehicleNumber,
                Capacity = vehicle.Capacity,
                Available = vehicle.Available ?? "Available"
            };

            return (true, "Vehicle updated successfully.", result);
        }

        public async Task<(bool Success, string Message)> DeleteVehicleAsync(int vehicleId)
        {
            var vehicle = await _repo.GetVehicleByIdAsync(vehicleId);
            if (vehicle is null)
            {
                return (false, "Vehicle not found.");
            }

            if (vehicle.Available == "Assigned")
            {
                return (false, "Cannot delete vehicle because it is currently assigned.");
            }

            var hasLinks = await _repo.HasLinksForVehicleAsync(vehicleId);
            if (hasLinks)
            {
                return (false, "Cannot delete vehicle because it is part of a driver-vehicle link.");
            }

            var deleted = await _repo.DeleteVehicleAsync(vehicleId);
            if (!deleted)
                return (false, $"Vehicle #{vehicleId} could not be deleted.");

            try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }

            return (true, $"Vehicle #{vehicleId} deleted successfully.");
        }

        // Drivers

        public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
        {
            var drivers = await _repo.GetDriversAsync();
            return drivers.Select(d => new DriverDto
            {
                DriverId = d.DriverId,
                DriversName = d.DriversName,
                LicenseNumber = d.LicenseNumber,
                Available = d.Available ?? "Available"
            });
        }

        public async Task<(bool Success, string Message, DriverDto? Data)> CreateDriverAsync(CreateDriverDto dto)
        {
            var existingDrivers = await _repo.GetDriversAsync();
            if (existingDrivers.Any(d => d.LicenseNumber == dto.LicenseNumber))
            {
                return (false, $"Driver with License Number {dto.LicenseNumber} already exists.", null);
            }

            var driver = new Driver
            {
                DriversName = dto.DriversName,
                LicenseNumber = dto.LicenseNumber,
                Available = dto.Available
            };

            await _repo.AddDriverAsync(driver);
            await _repo.SaveAsync();

            var result = new DriverDto
            {
                DriverId = driver.DriverId,
                DriversName = driver.DriversName,
                LicenseNumber = driver.LicenseNumber,
                Available = driver.Available ?? "Available"
            };

            return (true, "Driver added successfully.", result);
        }

        public async Task<(bool Success, string Message, DriverDto? Data)> UpdateDriverAsync(int driverId, UpdateDriverDto dto)
        {
            var driver = await _repo.GetDriverByIdAsync(driverId);
            if (driver is null)
            {
                return (false, "Driver not found.", null);
            }

            driver.DriversName = dto.DriversName;
            driver.LicenseNumber = dto.LicenseNumber;
            driver.Available = dto.Available;

            await _repo.SaveAsync();

            var result = new DriverDto
            {
                DriverId = driver.DriverId,
                DriversName = driver.DriversName,
                LicenseNumber = driver.LicenseNumber,
                Available = driver.Available ?? "Available"
            };

            return (true, "Driver updated successfully.", result);
        }

        public async Task<(bool Success, string Message)> DeleteDriverAsync(int driverId)
        {
            var driver = await _repo.GetDriverByIdAsync(driverId);
            if (driver is null)
            {
                return (false, "Driver not found.");
            }

            if (driver.Available == "Assigned")
            {
                return (false, "Cannot delete driver because they are currently assigned.");
            }

            var hasLinks = await _repo.HasLinksForDriverAsync(driverId);
            if (hasLinks)
            {
                return (false, "Cannot delete driver because they are part of a driver-vehicle link.");
            }

            var deleted = await _repo.DeleteDriverAsync(driverId);
            if (!deleted)
                return (false, $"Driver #{driverId} could not be deleted.");

            try
            {
                await _repo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                return (false, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }

            return (true, $"Driver #{driverId} deleted successfully.");
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
            Capacity = dvl.Vehicle?.Capacity,
            Status = dvl.Status ?? "Available"
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