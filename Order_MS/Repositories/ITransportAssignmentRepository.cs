using Order_MS.Models;

namespace Order_MS.Repositories
{
    public interface ITransportAssignmentRepository
    {

        Task<IEnumerable<OrderRequest>> GetApprovedOrderRequestsAsync();
        Task<OrderRequest?> GetOrderRequestWithDetailsAsync(int orderReqId);
        Task<bool> UpdateOrderRequestStatusAsync(int orderReqId, string newStatus);

        Task<IEnumerable<DriverVehicleLink>> GetAvailableDriverVehicleLinksAsync();
        Task<IEnumerable<TransportAssignment>> GetAllAssignmentsAsync();
        Task<IEnumerable<TransportAssignment>> GetAssignmentsByOrderRequestAsync(int orderReqId);
        Task<TransportAssignment?> GetAssignmentWithDetailsAsync(int assignmentId);
        Task AddAssignmentAsync(TransportAssignment assignment);
        Task<bool> UpdateAssignmentStatusAsync(int assignmentId, string newStatus);
        Task SaveAsync();
    }
}