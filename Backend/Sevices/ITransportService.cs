using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface ITransportService
    {
      
        Task<IEnumerable<OrderRequestForTransportDto>> GetApprovedOrderRequestsAsync();

        Task<IEnumerable<AvailableDriverVehicleLinkDto>> GetAvailableDriverVehicleLinksAsync();
        Task<IEnumerable<AvailableDriverVehicleLinkDto>> GetAllDriverVehicleLinksAsync();
        Task<(bool Success, string Message, AvailableDriverVehicleLinkDto? Data)> CreateDriverVehicleLinkAsync(CreateDriverVehicleLinkDto dto);
        Task<(bool Success, string Message)> DeleteDriverVehicleLinkAsync(int connectionId);

        Task<IEnumerable<TransportAssignmentResponseDto>> GetAllAssignmentsAsync();
        Task<IEnumerable<TransportAssignmentResponseDto>> GetAssignmentsByOrderRequestAsync(int orderReqId);

        Task<(bool Success, string Message, IEnumerable<TransportAssignmentResponseDto>? Data)>
            AssignTransportAsync(AssignTransportDto dto, int? userId = null);

        Task<(bool Success, string Message)>
            UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusDto dto);

        // Vehicles
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<(bool Success, string Message, VehicleDto? Data)> CreateVehicleAsync(CreateVehicleDto dto);
        Task<(bool Success, string Message, VehicleDto? Data)> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto dto);
        Task<(bool Success, string Message)> DeleteVehicleAsync(int vehicleId);

        // Drivers
        Task<IEnumerable<DriverDto>> GetAllDriversAsync();
        Task<(bool Success, string Message, DriverDto? Data)> CreateDriverAsync(CreateDriverDto dto);
        Task<(bool Success, string Message, DriverDto? Data)> UpdateDriverAsync(int driverId, UpdateDriverDto dto);
        Task<(bool Success, string Message)> DeleteDriverAsync(int driverId);
    }
}