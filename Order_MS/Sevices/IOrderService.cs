using Order_MS.DTOs;

namespace Order_MS.Sevices
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDtos>> GetAllOrders();

        Task<OrderResponseDtos?> GetOrder(int id);

        Task CreateOrder(OrderCreateDtos OrderDto, OrderLiCreateDtos);

        Task UpdateStudent(int id, StudentUpdateDto dto);

        Task DeleteStudent(int id);
    }
}
