using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Sevices;
using System.Security.Claims;

namespace Order_MS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllOrders();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderById(id);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        return Ok(order);
    }

    [HttpPost("from-request/{orderReqId}")]
    public async Task<IActionResult> CreateFromOrderRequest(int orderReqId)
    {
        var created = await _orderService.CreateOrderFromOrderRequest(orderReqId);
        return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDtos dto)
    {
        var existing = await _orderService.GetOrderById(id);
        if (existing == null)
            return NotFound(new { message = "Order not found" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var modifiedBy))
            return Unauthorized(new { message = "Invalid user context" });

        await _orderService.UpdateOrder(id, dto, modifiedBy);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _orderService.GetOrderById(id);
        if (existing == null)
            return NotFound(new { message = "Order not found" });

        await _orderService.DeleteOrder(id);
        return NoContent();
    }
}
