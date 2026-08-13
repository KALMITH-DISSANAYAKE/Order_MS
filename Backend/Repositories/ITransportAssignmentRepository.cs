using Order_MS.Models;

namespace Order_MS.Repositories
{
    public interface ITransportAssignmentRepository
    {

        Task<IEnumerable<OrderRequest>> GetApprovedOrderRequestsAsync();
        Task<OrderRequest?> GetOrderRequestWithDetailsAsync(int orderReqId);
        Task<bool> UpdateOrderRequestStatusAsync(int orderReqId, string newStatus);

        Task<IEnumerable<DriverVehicleLink>> GetAvailableDriverVehicleLinksAsync();
        Task<IEnumerable<DriverVehicleLink>> GetAllDriverVehicleLinksAsync();
        Task AddDriverVehicleLinkAsync(DriverVehicleLink link);
        Task<bool> DeleteDriverVehicleLinkAsync(int connectionId);

        Task<IEnumerable<TransportAssignment>> GetAllAssignmentsAsync();
        Task<IEnumerable<TransportAssignment>> GetAssignmentsByOrderRequestAsync(int orderReqId);
        Task<TransportAssignment?> GetAssignmentWithDetailsAsync(int assignmentId);
        Task AddAssignmentAsync(TransportAssignment assignment);
        Task<bool> UpdateAssignmentStatusAsync(int assignmentId, string newStatus);
        
        // Driver and Vehicle
        Task<IEnumerable<Vehicle>> GetVehiclesAsync();
        Task AddVehicleAsync(Vehicle vehicle);
        Task<IEnumerable<Driver>> GetDriversAsync();
        Task AddDriverAsync(Driver driver);

        Task SaveAsync();
    }
}