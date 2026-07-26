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

    public OrderService(IOrderRepository<Order> repository)
    {
        _repository = repository;
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
                    .ThenInclude(cv => cv.Vehical)
        );
        var orderDtos = orders.Select(order => new OrderResponseDtos
        {
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
                VehicalId = order.Connection.VehicalId,
                VehicalNumber = order.Connection.Vehical.VehicalNumber
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
                    .ThenInclude(c => c.Vehical)
        );

        if (order == null)
            return null;

        return new OrderResponseDtos
        {
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
                VehicalId = order.Connection.VehicalId,
                VehicalNumber = order.Connection.Vehical.VehicalNumber
            }
        };
    }




}