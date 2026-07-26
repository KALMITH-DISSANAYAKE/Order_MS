using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;

namespace Order_MS.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly OrderMSDbContext _context;

        public DeliveryService(OrderMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DeliveryListDto>> GetAllDeliveriesAsync()
        {
     
            var orders = await _context.Orders
                .Include(o => o.Connection)
                    .ThenInclude(dvl => dvl!.Driver)
                .Include(o => o.Connection)
                    .ThenInclude(dvl => dvl!.Vehicle)
                .Include(o => o.OrderReq)
                    .ThenInclude(or => or!.Branch)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();

            return orders.Select(o => new DeliveryListDto
            {
                OrderId = o.OrderId,
                OrderReqId = o.OrderReqId,
                OrderStatus = o.OrderStatus ?? string.Empty,
                Price = o.Price,
                DriverName = o.Connection?.Driver?.DriversName,
                VehicleNumber = o.Connection?.Vehicle?.VehicleNumber,
                BranchLocation = o.OrderReq?.Branch?.Location,
                CreatedOn = o.CreatedOn
            });
        }

        public async Task<DeliveryDetailDto?> GetDeliveryByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Connection)
                    .ThenInclude(dvl => dvl!.Driver)
                .Include(o => o.Connection)
                    .ThenInclude(dvl => dvl!.Vehicle)
                .Include(o => o.OrderReq)
                    .ThenInclude(or => or!.Branch)
                .Include(o => o.OrderLines)      
                    .ThenInclude(ol => ol.Item)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order is null) return null;

            return new DeliveryDetailDto
            {
                OrderId = order.OrderId,
                OrderReqId = order.OrderReqId,
                OrderStatus = order.OrderStatus ?? string.Empty,
                Price = order.Price,
                OrderRemark = order.OrderRemark,
                DriverName = order.Connection?.Driver?.DriversName,
                VehicleNumber = order.Connection?.Vehicle?.VehicleNumber,
                BranchLocation = order.OrderReq?.Branch?.Location,
                CreatedOn = order.CreatedOn,
                Lines = order.OrderLines
                    .Select(ol => new DeliveryLineDto
                    {
                        OrderLineId = ol.OrderlineId, 
                        ItemId = ol.ItemId,
                        ItemName = ol.Item?.ItemName ?? string.Empty,
                        SupplierId = ol.SupplierId,
                        Quantity = ol.Quantity,
                        UnitPrice = ol.UnitPrice,
                        TotalPrice = ol.TotalPrice
                    }).ToList()
            };
        }

        public async Task<(bool Success, string Message)>
            UpdateDeliveryStatusAsync(int orderId, UpdateDeliveryStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status cannot be empty.");

            var order = await _context.Orders.FindAsync(orderId);
            if (order is null) return (false, $"Order #{orderId} not found.");

            order.OrderStatus = dto.Status;
            order.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, $"Order #{orderId} status updated to '{dto.Status}'.");
        }
    }
}