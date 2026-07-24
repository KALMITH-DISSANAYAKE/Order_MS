using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface ITransportService
    {
        Task<TransportResponseDto> AssignTransportAsync(AssignTransportDto dto);
        Task<List<VehicleDto>> GetAvailableVehiclesAsync();
        Task<List<DriverDto>> GetAvailableDriversAsync();
        Task<List<TransportResponseDto>> GetAssignmentsAsync();
        Task<TransportResponseDto> GetAssignmentByOrderIdAsync(int orderId);
    }
}