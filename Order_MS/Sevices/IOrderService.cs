using Order_MS.DTOs;

namespace Order_MS.Sevices
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDtos>> GetAllOrders();

        Task<OrderResponseDtos?> GetOrderById(int id);

        Task CreateOrder(OrderCreateDtos OrderDto, OrderLiCreateDtos);

        Task UpdateOrder(int id, StudentUpdateDto dto);

        Task DeleteOrder(int id);
    }
}
