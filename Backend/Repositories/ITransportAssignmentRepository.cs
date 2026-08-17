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
        Task<bool> HasAssignmentsForLinkAsync(int connectionId);

        Task<IEnumerable<TransportAssignment>> GetAllAssignmentsAsync();
        Task<IEnumerable<TransportAssignment>> GetAssignmentsByOrderRequestAsync(int orderReqId);
        Task<TransportAssignment?> GetAssignmentWithDetailsAsync(int assignmentId);
        Task AddAssignmentAsync(TransportAssignment assignment);
        Task<bool> UpdateAssignmentStatusAsync(int assignmentId, string newStatus);
        
        // Driver and Vehicle
        Task<IEnumerable<Vehicle>> GetVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
        Task AddVehicleAsync(Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int vehicleId);
        Task<bool> HasLinksForVehicleAsync(int vehicleId);
        Task<IEnumerable<Driver>> GetDriversAsync();
        Task<Driver?> GetDriverByIdAsync(int driverId);
        Task AddDriverAsync(Driver driver);
        Task<bool> DeleteDriverAsync(int driverId);
        Task<bool> HasLinksForDriverAsync(int driverId);

        Task SaveAsync();
    }
}