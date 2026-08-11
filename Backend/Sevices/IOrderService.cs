using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDtos>> GetAllOrders();

        Task<OrderResponseDtos?> GetOrderById(int id);

        Task<OrderResponseDtos> CreateOrderFromOrderRequest(int orderReqId, int createdBy);

        Task CreateOrder(OrderCreateDtos OrderDto);

        Task UpdateOrder(int id, OrderUpdateDtos dto, int modifiedBy);

        Task DeleteOrder(int id);
    }
}
