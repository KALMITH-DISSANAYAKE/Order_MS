using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IDeliveryService
    {
        Task<IEnumerable<DeliveryListDto>> GetAllDeliveriesAsync();
        Task<DeliveryDetailDto?> GetDeliveryByIdAsync(int orderId);
        Task<(bool Success, string Message)> UpdateDeliveryStatusAsync(int orderId, UpdateDeliveryStatusDto dto);
    }
}