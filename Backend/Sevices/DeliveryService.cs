using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Exceptions;

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
            var allowedStatuses = new[] { "Approved", "TransportAssigned" };
            
            var requests = await _context.OrderRequests
                .Include(or => or.Branch)
                .Include(or => or.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Driver)
                .Include(or => or.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Vehicle)
                .Where(or => allowedStatuses.Contains(or.ReqStatus))
                .OrderByDescending(or => or.RequestedOn)
                .ToListAsync();

            return requests.Select(or => 
            {
                var assignment = or.TransportAssignments.OrderByDescending(a => a.AssignedOn).FirstOrDefault();
                return new DeliveryListDto
                {
                    OrderReqId = or.OrderReqId,
                    OrderStatus = or.ReqStatus ?? string.Empty,
                    Price = or.TotalPrice,
                    DriverName = assignment?.Connection?.Driver?.DriversName,
                    VehicleNumber = assignment?.Connection?.Vehicle?.VehicleNumber,
                    BranchLocation = or.Branch?.Location,
                    CreatedOn = or.RequestedOn
                };
            });
        }

        public async Task<DeliveryDetailDto?> GetDeliveryByIdAsync(int orderReqId)
        {
            var or = await _context.OrderRequests
                .Include(o => o.Branch)
                .Include(o => o.OrderRequestLines)
                    .ThenInclude(l => l.Item)
                .Include(o => o.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Driver)
                .Include(o => o.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Vehicle)
                .FirstOrDefaultAsync(o => o.OrderReqId == orderReqId);

            if (or is null)
                throw new BusinessException($"Order Request with ID {orderReqId} not found.", 404);

            var assignment = or.TransportAssignments.OrderByDescending(a => a.AssignedOn).FirstOrDefault();

            return new DeliveryDetailDto
            {
                OrderReqId = or.OrderReqId,
                OrderStatus = or.ReqStatus ?? string.Empty,
                Price = or.TotalPrice,
                OrderRemark = "", // OrderRequest might not have Remark
                DriverName = assignment?.Connection?.Driver?.DriversName,
                VehicleNumber = assignment?.Connection?.Vehicle?.VehicleNumber,
                BranchLocation = or.Branch?.Location,
                CreatedOn = or.RequestedOn,
                Lines = or.OrderRequestLines
                    .Select(ol => new DeliveryLineDto
                    {
                        OrderReqLineId = ol.OrderReqLineId, 
                        ItemId = ol.ItemId,
                        ItemName = ol.Item?.ItemName ?? string.Empty,
                        Quantity = ol.Quantity,
                        UnitPrice = ol.Price, 
                        TotalPrice = ol.Price * ol.Quantity
                    }).ToList()
            };
        }



        public async Task<(bool Success, string Message)> AssignDeliveryAsync(int orderReqId, AssignDeliveryDto dto)
        {
            var or = await _context.OrderRequests.FindAsync(orderReqId);
            if (or is null)
                throw new BusinessException($"Order Request #{orderReqId} not found.", 404);

            var link = await _context.DriverVehicleLinks.FindAsync(dto.ConnectionId);
            if (link is null)
                throw new BusinessException($"Driver-Vehicle Link #{dto.ConnectionId} not found.", 404);

            var assignment = new TransportAssignment
            {
                OrderReqId = orderReqId,
                ConnectionId = dto.ConnectionId,
                AssignedOn = DateTime.UtcNow,
                Status = "Assigned",
                Quantity = or.TotalQuantity ?? 0
            };

            await _context.TransportAssignments.AddAsync(assignment);
            or.ReqStatus = "TransportAssigned";
            or.ModifiedOn = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException($"Database error: {ex.InnerException?.Message ?? ex.Message}", 400);
            }

            return (true, $"Delivery #{orderReqId} successfully assigned.");
        }
    }
}