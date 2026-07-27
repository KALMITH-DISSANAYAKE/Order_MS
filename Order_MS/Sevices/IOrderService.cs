using Order_MS.DTOs;

namespace Order_MS.Sevices
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDtos>> GetAllOrders();

        Task<OrderResponseDtos?> GetOrderById(int id);

        Task<OrderResponseDtos> CreateOrderFromOrderRequest(int orderReqId);

        Task CreateOrder(OrderCreateDtos OrderDto);

        Task UpdateOrder(int id, OrderUpdateDtos dto, int modifiedBy);

        Task DeleteOrder(int id);
    }
}
