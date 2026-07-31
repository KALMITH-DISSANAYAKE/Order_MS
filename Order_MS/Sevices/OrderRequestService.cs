using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Interfaces;
using Order_MS.Models;
using Order_MS.Repositories;
using Order_MS.Sevices;
using System.Security.Claims;

namespace Order_MS.Services;

public class OrderRequestService : IOrderRequestService
{
    private readonly OrderMSDbContext _context;
    private readonly IOrderService _orderService;


    public OrderRequestService(
    OrderMSDbContext context,
    IOrderService orderService)
    {
        _context = context;
        _orderService = orderService;
    }

    public async Task<OrderRequestResponseDTO> CreateOrderRequest(CreateOrderRequestDTO dto)
    {
        // Look up the requesting user to get their BranchId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.RequestedBy);

        if (user == null)
            throw new Exception($"User with ID {dto.RequestedBy} not found");

        if (user.BranchId == null)
            throw new Exception($"User {user.UserName} is not assigned to any branch");

        var orderRequest = new OrderRequest
        {
            RequestedBy = dto.RequestedBy,
            BranchId = user.BranchId.Value,
            ReqStatus = "SubmittedForReview",
            RequestedOn = DateTime.Now
        };

        _context.OrderRequests.Add(orderRequest);

        await _context.SaveChangesAsync();

        int totalQuantity = 0;
        decimal totalPrice = 0;

        foreach (var item in dto.Items)
        {

            var dbItem = await _context.Items
                .FirstOrDefaultAsync(x => x.ItemId == item.ItemId);

            if (dbItem == null)
            {
                throw new Exception($"Item with ID {item.ItemId} not found");
            }

            var orderRequestLine = new OrderRequestLine
            {
                OrderReqId = orderRequest.OrderReqId,
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                Price = dbItem.UnitPrice
            };


            _context.OrderRequestLines.Add(orderRequestLine);

            totalQuantity += item.Quantity;

            totalPrice += item.Quantity * dbItem.UnitPrice;


        }

        orderRequest.TotalQuantity = totalQuantity;

        orderRequest.TotalPrice = totalPrice;

        _context.OrderRequests.Update(orderRequest);

        await _context.SaveChangesAsync();

        // Reload with lines and items to build the response
        var savedRequest = await _context.OrderRequests
            .Include(x => x.OrderRequestLines)
            .ThenInclude(x => x.Item)
            .FirstAsync(x => x.OrderReqId == orderRequest.OrderReqId);

        return new OrderRequestResponseDTO
        {
            OrderReqId = savedRequest.OrderReqId,
            Status = savedRequest.ReqStatus,
            TotalQuantity = savedRequest.TotalQuantity ?? 0,
            TotalPrice = savedRequest.TotalPrice ?? 0,
            RequestedOn = savedRequest.RequestedOn ?? DateTime.Now,
            Items = savedRequest.OrderRequestLines.Select(line => new OrderRequestLineResponseDTO
            {
                ItemId = line.ItemId,
                ItemName = line.Item.ItemName,
                Quantity = line.Quantity,
                UnitPrice = line.Price ?? 0,
                LineTotal = line.Quantity * (line.Price ?? 0)
            }).ToList()
        };
    }

    public async Task<List<OrderRequestListDTO>> GetAllOrderRequests()
    {
        var requests = await _context.OrderRequests
         .Include(x => x.RequestedByNavigation)
            .Select(x => new OrderRequestListDTO
            {
                OrderReqId = x.OrderReqId,
                Status = x.ReqStatus,
                TotalQuantity = x.TotalQuantity ?? 0,
                TotalPrice = x.TotalPrice ?? 0,
                RequestedOn = x.RequestedOn ?? DateTime.MinValue,
                RequestedBy = x.RequestedByNavigation.UserName
            })
            .ToListAsync();
        return requests;


    }

    public async Task<OrderRequestResponseDTO?> GetOrderRequestById(int id)
    {
        var request = await _context.OrderRequests
            .Include(x => x.OrderRequestLines)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderReqId == id);

        if (request == null)
        {
            return null;
        }

        return new OrderRequestResponseDTO
        {
            OrderReqId = request.OrderReqId,
            Status = request.ReqStatus,
            TotalQuantity = request.TotalQuantity ?? 0,
            TotalPrice = request.TotalPrice ?? 0,
            RequestedOn = request.RequestedOn ?? DateTime.Now,

            Items = request.OrderRequestLines
               .Select(line => new OrderRequestLineResponseDTO
               {
                   ItemId = line.ItemId,
                   ItemName = line.Item.ItemName,
                   Quantity = line.Quantity,
                   UnitPrice = line.Price ?? 0,
                   LineTotal = line.Quantity * line.Price ?? 0
               })
                .ToList()
        };
    }

    public async Task<OrderRequestResponseDTO?> ApproveOrderRequest(int id, int approvedBy)
    {
        var request = await _context.OrderRequests
            .Include(x => x.OrderRequestLines)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderReqId == id);

        if (request == null)
        {
            return null;
        }

        if (request.ReqStatus != "SubmittedForReview")
        {
            return null;
        }

        request.ReqStatus = "Approved";
        request.ApprovedBy = approvedBy;
        request.ApprovedOn = DateTime.Now;

        var transportAssignment = new TransportAssignment
        {
            OrderReqId = request.OrderReqId,
            Status = "Pending",
            AssignedOn = DateTime.Now
        };

        _context.TransportAssignments.Add(transportAssignment);

        await _context.SaveChangesAsync();

        return new OrderRequestResponseDTO
        {
            OrderReqId = request.OrderReqId,
            Status = request.ReqStatus,
            TotalQuantity = request.TotalQuantity ?? 0,
            TotalPrice = request.TotalPrice ?? 0,
            RequestedOn = request.RequestedOn ?? DateTime.Now,
            Items = request.OrderRequestLines.Select(line => new OrderRequestLineResponseDTO
            {
                ItemId = line.ItemId,
                ItemName = line.Item.ItemName,
                Quantity = line.Quantity,
                UnitPrice = line.Price ?? 0,
                LineTotal = line.Quantity * (line.Price ?? 0)
            }).ToList()
        };

    }

    public async Task<OrderRequestResponseDTO?> RejectOrderRequest(int id)
    {
        var request = await _context.OrderRequests
            .Include(x => x.OrderRequestLines)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderReqId == id);

        if (request == null)
        {
            return null;
        }

        if (request.ReqStatus != "SubmittedForReview")
        {
            return null;
        }

        request.ReqStatus = "Rejected";

        await _context.SaveChangesAsync();

        return new OrderRequestResponseDTO
        {
            OrderReqId = request.OrderReqId,
            Status = request.ReqStatus,
            TotalQuantity = request.TotalQuantity ?? 0,
            TotalPrice = request.TotalPrice ?? 0,
            RequestedOn = request.RequestedOn ?? DateTime.Now,
            Items = request.OrderRequestLines.Select(line => new OrderRequestLineResponseDTO
            {
                ItemId = line.ItemId,
                ItemName = line.Item.ItemName,
                Quantity = line.Quantity,
                UnitPrice = line.Price ?? 0,
                LineTotal = line.Quantity * (line.Price ?? 0)
            }).ToList()
        };
    }

    public async Task<OrderRequestResponseDTO?> MakePayment(int id)
    {
        var request = await _context.OrderRequests
            .Include(x => x.OrderRequestLines)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderReqId == id);

        if (request == null)
        {
            return null;
        }

        if (request.ReqStatus != "TransportAssigned")
        {
            return null;
        }

        request.ReqStatus = "PaymentSuccessful";

        await _context.SaveChangesAsync();
        //added here the order creation logic after payment

        await _orderService.CreateOrderFromOrderRequest(
        request.OrderReqId,
        (int)request.ApprovedBy);


        return new OrderRequestResponseDTO
        {
            OrderReqId = request.OrderReqId,
            Status = request.ReqStatus,
            TotalQuantity = request.TotalQuantity ?? 0,
            TotalPrice = request.TotalPrice ?? 0,
            RequestedOn = request.RequestedOn ?? DateTime.Now,
            Items = request.OrderRequestLines.Select(line => new OrderRequestLineResponseDTO
            {
                ItemId = line.ItemId,
                ItemName = line.Item.ItemName,
                Quantity = line.Quantity,
                UnitPrice = line.Price ?? 0,
                LineTotal = line.Quantity * (line.Price ?? 0)
            }).ToList()
        };
    }
}
