using Order_MS.DTOs;

namespace Order_MS.Interfaces;

public interface IOrderRequestService
{
    Task<OrderRequestResponseDTO> CreateOrderRequest(CreateOrderRequestDTO dto);

    Task<List<OrderRequestListDTO>> GetAllOrderRequests();

    Task<OrderRequestResponseDTO?> GetOrderRequestById(int id);
}