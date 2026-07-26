using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface ITransportService
    {
      
        Task<IEnumerable<OrderRequestForTransportDto>> GetApprovedOrderRequestsAsync();

        Task<IEnumerable<AvailableDriverVehicleLinkDto>> GetAvailableDriverVehicleLinksAsync();

        Task<IEnumerable<TransportAssignmentResponseDto>> GetAllAssignmentsAsync();
        Task<IEnumerable<TransportAssignmentResponseDto>> GetAssignmentsByOrderRequestAsync(int orderReqId);

        Task<(bool Success, string Message, IEnumerable<TransportAssignmentResponseDto>? Data)>
            AssignTransportAsync(AssignTransportDto dto);

        Task<(bool Success, string Message)>
            UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusDto dto);
    }
}