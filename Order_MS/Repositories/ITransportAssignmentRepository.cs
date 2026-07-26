using Order_MS.Models;

namespace Order_MS.Repositories
{
    public interface ITransportAssignmentRepository : IGenericRepository<TransportAssignment>
    {
        Task<TransportAssignment?> GetByOrderIdAsync(int orderId);
        Task<TransportAssignment?> GetByOrderIdWithDetailsAsync(int orderId);
        Task<List<TransportAssignment>> GetAllWithDetailsAsync();
        Task<List<TransportAssignment>> GetDeliveredOrInTransitAsync();
    }
}