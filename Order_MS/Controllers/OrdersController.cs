using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Exceptions;
using Order_MS.Sevices;
using System.ComponentModel.DataAnnotations;
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
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving orders." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving the order." });
        }
    }

    [HttpPost("from-request/{orderReqId}")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> CreateFromOrderRequest(int orderReqId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var createdBy))
                throw new UnauthorizedAccessException("Invalid user context");

            var created = await _orderService.CreateOrderFromOrderRequest(orderReqId, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while creating the order." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "BranchManager")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDtos dto)
    {
        try
        {
            if (dto == null)
                throw new ValidationException("Request body is required.");

            Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

            var existing = await _orderService.GetOrderById(id);
            if (existing == null)
                return NotFound(new { message = "Order not found" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var modifiedBy))
                throw new UnauthorizedAccessException("Invalid user context");

            await _orderService.UpdateOrder(id, dto, modifiedBy);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while updating the order." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var existing = await _orderService.GetOrderById(id);
            if (existing == null)
                return NotFound(new { message = "Order not found" });

            await _orderService.DeleteOrder(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred while deleting the order." });
        }
    }
}
