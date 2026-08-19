using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Exceptions;
using Order_MS.Interfaces;
using Order_MS.Models;
using Order_MS.Repositories;
using Order_MS.Services;

namespace Order_MS.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository<Order> _repository;
    private readonly OrderMSDbContext _context;

    public OrderService(IOrderRepository<Order> repository, OrderMSDbContext context)
    {
        _repository = repository;
        _context = context;
    }
    public async Task<IEnumerable<OrderResponseDtos>> GetAllOrders()
    {
        try
        {

            var orders = await _repository.GetAllAsync(query =>
                query
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Item)
                    .Include(o => o.Connection)
                        .ThenInclude(cv => cv.Driver)
                    .Include(o => o.Connection)
                        .ThenInclude(cv => cv.Vehicle)
                    .Include(o => o.OrderRequestedByNavigation)
                    .Include(o => o.OrderBranchNavigation)
                    .Include(o => o.OrderReq)
                        .ThenInclude(or => or.RequestedByNavigation)
                    .Include(o => o.OrderReq)
                        .ThenInclude(or => or.Branch)
            );
            var orderDtos = orders.Select(order => new OrderResponseDtos
            {
             
                OrderId = order.OrderId,
                OrderReqId = order.OrderReqId,
                OrderStatus = order.OrderStatus,
                OrderRequestedBy = order.OrderRequestedByNavigation != null
                    ? $"{order.OrderRequestedByNavigation.FirstName} {order.OrderRequestedByNavigation.LastName}".Trim()
                    : (order.OrderReq?.RequestedByNavigation != null
                        ? $"{order.OrderReq.RequestedByNavigation.FirstName} {order.OrderReq.RequestedByNavigation.LastName}".Trim()
                        : null),
                OrderBranchId = order.OrderBranch ?? order.OrderReq?.BranchId,
                OrderBranch = order.OrderBranchNavigation?.BranchCode ?? order.OrderReq?.Branch?.BranchCode,
                CreatedBy = order.CreatedBy,
                CreatedOn = order.CreatedOn,
                OrderRemark = order.OrderRemark,
                OrderLines = order.OrderLines.Select(ol => new OrderLineToOrderDtos
                {
                    OrderlineId = ol.OrderlineId,
                    ItemId = ol.ItemId,
                    ItemName = ol.Item.ItemName,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice,
                    TotalPrice = ol.TotalPrice,
                }),
                Total = order.Price,
                ConnectionLine = order.Connection == null ? null : new TransportToOrderDtos
                {
                    DriverId = order.Connection.DriverId,
                    DriversName = order.Connection.Driver.DriversName,
                    DriverLicenseNumber = order.Connection.Driver.LicenseNumber,
                    VehicalId = order.Connection.VehicleId,
                    VehicalNumber = order.Connection.Vehicle.VehicleNumber
                }
            }).ToArray();

            return orderDtos;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Database error while retrieving orders: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new BusinessException("An unexpected error occurred while retrieving orders.", 500);
        }
    }

    public async Task<OrderResponseDtos?> GetOrderById(int id)
    {
        if (id <= 0)
            throw new BusinessException("Order ID must be greater than zero.", 400);

        try
        {
            var order = await _repository.GetAsync(
                o => o.OrderId == id,
                query => query
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Item)
                    .Include(o => o.Connection)
                        .ThenInclude(c => c.Driver)
                    .Include(o => o.Connection)
                        .ThenInclude(c => c.Vehicle)
                    .Include(o => o.OrderReq)
                        .ThenInclude(or => or.RequestedByNavigation)
                    .Include(o => o.OrderReq)
                        .ThenInclude(or => or.Branch)
            );

            if (order == null)
                return null;

            return new OrderResponseDtos
            {
                OrderId = order.OrderId,
                OrderReqId = order.OrderReqId,
                OrderStatus = order.OrderStatus,
                OrderRequestedBy = order.OrderRequestedByNavigation != null
                    ? $"{order.OrderRequestedByNavigation.FirstName} {order.OrderRequestedByNavigation.LastName}".Trim()
                    : (order.OrderReq?.RequestedByNavigation != null
                        ? $"{order.OrderReq.RequestedByNavigation.FirstName} {order.OrderReq.RequestedByNavigation.LastName}".Trim()
                        : null),
                OrderBranchId = order.OrderBranch ?? order.OrderReq?.BranchId,
                OrderBranch = order.OrderBranchNavigation?.BranchCode ?? order.OrderReq?.Branch?.BranchCode,
                CreatedBy = order.CreatedBy,
                CreatedOn = order.CreatedOn,
                OrderRemark = order.OrderRemark,
                Total = order.Price,

                OrderLines = order.OrderLines.Select(ol => new OrderLineToOrderDtos
                {
                    OrderlineId = ol.OrderlineId,
                    ItemId = ol.ItemId,
                    ItemName = ol.Item.ItemName,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.UnitPrice,
                    TotalPrice = ol.TotalPrice,
                }),

                ConnectionLine = order.Connection == null ? null : new TransportToOrderDtos
                {
                    DriverId = order.Connection.DriverId,
                    DriversName = order.Connection.Driver.DriversName,
                    DriverLicenseNumber = order.Connection.Driver.LicenseNumber,
                    VehicalId = order.Connection.VehicleId,
                    VehicalNumber = order.Connection.Vehicle.VehicleNumber
                }
            };
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Database error while retrieving the order: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new BusinessException("An unexpected error occurred while retrieving the order.", 500);
        }
    }

    public async Task<OrderResponseDtos> CreateOrderFromOrderRequest(int orderReqId, int createdBy)
    {
        if (orderReqId <= 0)
            throw new BusinessException("Order request ID must be greater than zero.", 400);

        if (createdBy <= 0)
            throw new BusinessException("Created by user ID must be greater than zero.", 400);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRequest = await _context.OrderRequests
                .Include(or => or.OrderRequestLines)
                    .ThenInclude(orl => orl.Item)
                .Include(or => or.RequestedByNavigation)
                .Include(or => or.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Driver)
                .Include(or => or.TransportAssignments)
                    .ThenInclude(ta => ta.Connection)
                        .ThenInclude(c => c!.Vehicle)
                .FirstOrDefaultAsync(or => or.OrderReqId == orderReqId);

            if (orderRequest == null)
                throw new BusinessException($"Order request {orderReqId} was not found.", 404);

            // Fetch the transport assignment directly to ensure we get the latest ConnectionId
            var selectedConnectionId = await _context.TransportAssignments
                .Where(ta => ta.OrderReqId == orderReqId && ta.ConnectionId.HasValue)
                .Select(ta => ta.ConnectionId)
                .FirstOrDefaultAsync();
        
            var order = new Order
            {
                OrderReqId = orderRequest.OrderReqId,
                ConnectionId = selectedConnectionId,
                Price = orderRequest.TotalPrice,
                OrderStatus = "InTransit",
                OrderRequestedBy = orderRequest.RequestedBy,
                OrderBranch = orderRequest.BranchId,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = createdBy,
                ModifiedOn = DateTime.UtcNow,
                OrderRemark = orderRequest.OrderReqRemark
            };

            await _repository.AddAsync(order);
            await _repository.SaveAsync();

            foreach (var requestLine in orderRequest.OrderRequestLines)
            {
                var unitPrice = requestLine.Price ?? requestLine.Item.UnitPrice;

                _context.OrderLines.Add(new OrderLine
                {
                    OrderId = order.OrderId,
                    ItemId = requestLine.ItemId,
                    SupplierId = requestLine.Item.SupplierId,
                    Quantity = requestLine.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * requestLine.Quantity
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetOrderById(order.OrderId))!;
        }
        catch (BusinessException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException($"Database error while creating the order: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException("An unexpected error occurred while creating the order.", 500);
        }
    }

    public async Task CreateOrder(OrderCreateDtos OrderDto)
    {
        if (OrderDto == null)
            throw new BusinessException("Order payload is required.", 400);

        if (OrderDto.OrderReqId == null || OrderDto.OrderReqId <= 0)
            throw new BusinessException("OrderReqId is required and must be greater than zero.", 400);

        if (OrderDto.CreatedBy <= 0)
            throw new BusinessException("CreatedBy must be greater than zero.", 400);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRequest = await _context.OrderRequests
                .Include(or => or.OrderRequestLines)
                    .ThenInclude(orl => orl.Item)
                .FirstOrDefaultAsync(or => or.OrderReqId == OrderDto.OrderReqId.Value);

            if (orderRequest == null)
                throw new BusinessException($"Order request {OrderDto.OrderReqId} was not found.", 404);

            var order = new Order
            {
                OrderReqId = OrderDto.OrderReqId,
                ConnectionId = OrderDto.ConnectionId,
                Price = OrderDto.Price,
                OrderStatus = OrderDto.OrderStatus,
                CreatedBy = OrderDto.CreatedBy,
                CreatedOn = OrderDto.CreatedOn,
                ModifiedBy = OrderDto.ModifiedBy,
                ModifiedOn = OrderDto.ModifiedOn,
                OrderRemark = OrderDto.OrderRemark
            };

            await _repository.AddAsync(order);
            await _repository.SaveAsync();

            var orderLineDtos = orderRequest.OrderRequestLines.Select(line => new OrderLineCreateDtos
            {
                OrderId = order.OrderId,
                ItemId = line.ItemId,
                SupplierId = line.Item.SupplierId,
                Quantity = line.Quantity,
                UnitPrice = line.Price ?? line.Item.UnitPrice,
                TotalPrice = (line.Price ?? line.Item.UnitPrice) * line.Quantity
            }).ToList();

            foreach (var orderLineDto in orderLineDtos)
            {
                var orderLine = new OrderLine
                {
                    OrderId = orderLineDto.OrderId,
                    ItemId = orderLineDto.ItemId,
                    SupplierId = orderLineDto.SupplierId,
                    Quantity = orderLineDto.Quantity,
                    UnitPrice = orderLineDto.UnitPrice,
                    TotalPrice = orderLineDto.TotalPrice
                };

                _context.OrderLines.Add(orderLine);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (BusinessException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException($"Database error while creating the order: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException("An unexpected error occurred while creating the order.", 500);
        }
    }

    public async Task UpdateOrder(int id, OrderUpdateDtos dto, int modifiedBy)
    {
        if (id <= 0)
            throw new BusinessException("Order ID must be greater than zero.", 400);

        if (dto == null)
            throw new BusinessException("Order update payload is required.", 400);

        if (string.IsNullOrWhiteSpace(dto.OrderRemark))
            throw new BusinessException("Order remark is required.", 400);

        if (modifiedBy <= 0)
            throw new BusinessException("Modified by user ID must be greater than zero.", 400);

        try
        {
            var order = await _repository.GetAsync(
                o => o.OrderId == id,
                query => query
                    .Include(o => o.OrderLines)
                    .Include(o => o.OrderReq)
            );

            if (order == null)
                throw new BusinessException($"Order with ID {id} was not found.", 404);

            order.OrderRemark = dto.OrderRemark;

            if (!string.IsNullOrEmpty(dto.OrderStatus))
            {
                order.OrderStatus = dto.OrderStatus.Trim();

                // When order status is Delivered
                if (string.Equals(order.OrderStatus, "Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    int? branchId = order.OrderBranch;

                    if (!branchId.HasValue && order.OrderReqId.HasValue)
                    {
                        var req = await _context.OrderRequests.FindAsync(order.OrderReqId.Value);
                        branchId = req?.BranchId;
                    }

                    if (branchId.HasValue)
                    {
                        // Load lines directly to ensure we get all items
                        var lines = await _context.OrderLines
                            .Where(ol => ol.OrderId == order.OrderId)
                            .ToListAsync();

                        // If OrderLines table has no lines for this order, get from OrderRequestLines
                        if (!lines.Any() && order.OrderReqId.HasValue)
                        {
                            var reqLines = await _context.OrderRequestLines
                                .Where(rl => rl.OrderReqId == order.OrderReqId.Value)
                                .ToListAsync();

                            foreach (var rl in reqLines)
                            {
                                lines.Add(new OrderLine
                                {
                                    OrderId = order.OrderId,
                                    ItemId = rl.ItemId,
                                    Quantity = rl.Quantity,
                                    UnitPrice = rl.Price ?? 0,
                                    TotalPrice = (rl.Price ?? 0) * rl.Quantity
                                });
                            }
                        }

                        foreach (var line in lines)
                        {
                            var branchInventory = await _context.Inventories
                                .FirstOrDefaultAsync(i => i.BranchId == branchId.Value && i.ItemId == line.ItemId);

                            if (branchInventory != null)
                            {
                                branchInventory.Quantity += line.Quantity;
                                branchInventory.ModifiedOn = DateTime.Now;
                                _context.Inventories.Update(branchInventory);
                            }
                            else
                            {
                                var newInventory = new Inventory
                                {
                                    BranchId = branchId.Value,
                                    ItemId = line.ItemId,
                                    Quantity = line.Quantity,
                                    ReorderLevel = 0,
                                    ModifiedOn = DateTime.Now
                                };
                                await _context.Inventories.AddAsync(newInventory);
                            }
                        }
                    }

                    // Release assigned Driver & Vehicle
                    if (order.ConnectionId.HasValue)
                    {
                        var connection = await _context.DriverVehicleLinks
                            .Include(c => c.Driver)
                            .Include(c => c.Vehicle)
                            .FirstOrDefaultAsync(c => c.ConnectionId == order.ConnectionId.Value);

                        if (connection != null)
                        {
                            if (connection.Driver != null)
                                connection.Driver.Available = "Available";
                            if (connection.Vehicle != null)
                                connection.Vehicle.Available = "Available";
                        }
                    }
                }
            }

            order.ModifiedBy = modifiedBy;
            order.ModifiedOn = DateTime.UtcNow;

            _repository.Update(order);
            await _context.SaveChangesAsync();
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Database error while updating the order: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new BusinessException("An unexpected error occurred while updating the order.", 500);
        }
    }

    public async Task DeleteOrder(int id)
    {
        if (id <= 0)
            throw new BusinessException("Order ID must be greater than zero.", 400);

        try
        {
            var order = await _repository.GetAsync(o => o.OrderId == id);

            if (order == null)
                throw new BusinessException($"Order with ID {id} was not found.", 404);

            _repository.Delete(order);
            await _repository.SaveAsync();
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException($"Database error while deleting the order: {ex.InnerException?.Message ?? ex.Message}", 500);
        }
        catch (Exception ex)
        {
            throw new BusinessException("An unexpected error occurred while deleting the order.", 500);
        }
    }




}