using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Exceptions;
using Order_MS.Interfaces;
using Order_MS.Models;
using Order_MS.Repositories;
using Order_MS.Services;
using Order_MS.Middleware;

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
      
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.RequestedBy);

        if (user == null)
        {
            throw new BusinessException(
                $"User with ID {dto.RequestedBy} not found.",
                404);
        }

        if (user.BranchId == null)
        {
            throw new BusinessException(
                $"User {user.UserName} is not assigned to any branch.",
                400);
        }

        var duplicateItemIds = dto.Items
            .GroupBy(x => x.ItemId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateItemIds.Any())
        {
            throw new BusinessException(
                $"Duplicate item IDs are not allowed: {string.Join(", ", duplicateItemIds)}.",
                400);
        }

        var itemIds = dto.Items
            .Select(x => x.ItemId)
            .ToList();

        var dbItems = await _context.Items
                .Where(x => itemIds.Contains(x.ItemId))
                .ToListAsync();

        var missingItemIds = itemIds
            .Except(dbItems.Select(x => x.ItemId))
            .ToList();

        if (missingItemIds.Any())
        {
            throw new BusinessException(
                $"The following item IDs were not found: {string.Join(", ", missingItemIds)}.",
                404);
        }

        var orderRequest = new OrderRequest
        {
            RequestedBy = dto.RequestedBy,
            BranchId = user.BranchId.Value,
            ReqStatus = "SubmittedForReview",
            RequestedOn = DateTime.Now
        };

        _context.OrderRequests.Add(orderRequest);

        int totalQuantity = 0;
        decimal totalPrice = 0;

        foreach (var item in dto.Items)
        {
            var dbItem = dbItems
                .First(x => x.ItemId == item.ItemId);

            var orderRequestLine = new OrderRequestLine
            {
                OrderReq = orderRequest,
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

        await _context.SaveChangesAsync();

  
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
            throw new BusinessException(
                $"Order request with ID {id} was not found.",
                404);
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
            throw new BusinessException(
                $"Order request with ID {id} was not found.",
                404);
        }

        if (request.ReqStatus != "SubmittedForReview")
        {
            throw new BusinessException(
                "Only order requests submitted for review can be approved.",
                400);
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
            throw new BusinessException(
                $"Order request with ID {id} was not found.",
                404);
        }

        if (request.ReqStatus != "SubmittedForReview")
        {
            throw new BusinessException(
                "Only order requests submitted for review can be rejected.",
                400);
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
            throw new BusinessException(
                $"Order request with ID {id} was not found.",
                404);
        }

        if (request.ReqStatus != "TransportAssigned")
        {
            throw new BusinessException(
                "Payment can only be made after transport has been assigned.",
                400);
        }

        request.ReqStatus = "PaymentSuccessful";

        await _context.SaveChangesAsync();

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
