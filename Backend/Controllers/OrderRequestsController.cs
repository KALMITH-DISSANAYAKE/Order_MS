using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Interfaces;

namespace Order_MS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderRequestController : ControllerBase
{
    private readonly IOrderRequestService _orderRequestService;

    public OrderRequestController(IOrderRequestService service)
    {
        _orderRequestService = service;
    }

    [HttpPost]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> CreateOrderRequest(
        CreateOrderRequestDTO dto)
    {
        var result = await _orderRequestService.CreateOrderRequest(dto);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "BranchManager,InventoryManager")]
    public async Task<IActionResult> GetAllOrderRequests()
    {
        var result = await _orderRequestService.GetAllOrderRequests();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "BranchManager,InventoryManager")]
    public async Task<IActionResult> GetOrderRequestById(int id)
    {
        var result = await _orderRequestService.GetOrderRequestById(id);

        return Ok(result);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> ApproveOrderRequest(
        int id,
        ApproveOrderRequestDTO dto)
    {
        var result = await _orderRequestService
            .ApproveOrderRequest(id, dto.ApprovedBy);

        return Ok(result);
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> RejectOrderRequest(int id)
    {
        var result = await _orderRequestService
            .RejectOrderRequest(id);

        return Ok(result);
    }

    [HttpPut("{id}/payment")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> MakePayment(int id)
    {
        var result = await _orderRequestService
            .MakePayment(id);

        return Ok(result);
    }
}