using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Interfaces;
using Order_MS.Models;
using Order_MS.Repositories;
using Order_MS.Sevices;

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
        var orders = await _repository.GetAllAsync(query =>
            query
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Item)
                .Include(o => o.Connection)
                    .ThenInclude(cv => cv.Driver)
                .Include(o => o.Connection)
                    .ThenInclude(cv => cv.Vehicle)
        );
        var orderDtos = orders.Select(order => new OrderResponseDtos
        {
            OrderId = order.OrderId,
            OrderReqId = order.OrderReqId,
            OrderStatus = order.OrderStatus,
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

    public async Task<OrderResponseDtos?> GetOrderById(int id)
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
        );

        if (order == null)
            return null;

        return new OrderResponseDtos
        {
            OrderId = order.OrderId,
            OrderReqId = order.OrderReqId,
            OrderStatus = order.OrderStatus,
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

    public async Task<OrderResponseDtos> CreateOrderFromOrderRequest(int orderReqId, int createdBy)
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
            throw new InvalidOperationException($"Order request {orderReqId} was not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var selectedConnectionId = orderRequest.TransportAssignments
            .FirstOrDefault(ta => ta.ConnectionId.HasValue)
            ?.ConnectionId;

        var order = new Order
        {
            OrderReqId = orderRequest.OrderReqId,
            ConnectionId = selectedConnectionId,
            Price = orderRequest.TotalPrice,
            OrderStatus = "Pending",
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

    public async Task CreateOrder(OrderCreateDtos OrderDto)
    {
        if (OrderDto.OrderReqId == null)
            throw new ArgumentException("OrderReqId is required.");

        var orderRequest = await _context.OrderRequests
            .Include(or => or.OrderRequestLines)
                .ThenInclude(orl => orl.Item)
            .FirstOrDefaultAsync(or => or.OrderReqId == OrderDto.OrderReqId.Value);

        if (orderRequest == null)
            throw new InvalidOperationException($"Order request {OrderDto.OrderReqId} was not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

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

    public async Task UpdateOrder(int id, OrderUpdateDtos dto, int modifiedBy)
    {
        var order = await _repository.GetAsync(o => o.OrderId == id);

        if (order == null)
            return;

        order.OrderRemark = dto.OrderRemark;
        if (!string.IsNullOrEmpty(dto.OrderStatus))
            order.OrderStatus = dto.OrderStatus;

        order.ModifiedBy = modifiedBy;
        order.ModifiedOn = DateTime.UtcNow;

        _repository.Update(order);
        await _repository.SaveAsync();
    }

    public async Task DeleteOrder(int id)
    {
        var order = await _repository.GetAsync(o => o.OrderId == id);

        if (order == null)
            return;

        _repository.Delete(order);
        await _repository.SaveAsync();
    }




}