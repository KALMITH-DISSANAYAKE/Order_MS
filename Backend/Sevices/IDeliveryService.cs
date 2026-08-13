using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IDeliveryService
    {
        Task<IEnumerable<DeliveryListDto>> GetAllDeliveriesAsync();
        Task<DeliveryDetailDto?> GetDeliveryByIdAsync(int orderReqId);
        Task<(bool Success, string Message)> AssignDeliveryAsync(int orderReqId, AssignDeliveryDto dto);
    }
}