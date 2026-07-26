using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IDeliveryService
    {
        Task<DeliveryResponseDto> VerifyDeliveryAsync(VerifyDeliveryDto dto);
        Task<DeliveryResponseDto> UpdateDeliveryStatusAsync(UpdateDeliveryStatusDto dto);
        Task<List<DeliveryHistoryDto>> GetDeliveryHistoryAsync(int? branchId = null);
        Task<DeliveryResponseDto> ConfirmDeliveryAsync(int orderId);
    }
}