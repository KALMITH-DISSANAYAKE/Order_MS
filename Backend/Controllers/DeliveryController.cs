using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;
using System.Security.Claims;
using Order_MS.Exceptions;


namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpGet]
        //[Authorize(Roles = "DeliveryDepartment")]   
        public async Task<IActionResult> GetAllDeliveries()
        {
            try
            {
            var result = await _deliveryService.GetAllDeliveriesAsync();
            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching all deliveries", details = ex.Message });
            }   
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = "DeliveryDepartment,InventoryManager")]
        public async Task<IActionResult> GetDeliveryById(int id)
        {
            try
            {
            var result = await _deliveryService.GetDeliveryByIdAsync(id);
            if (result is null)
                return NotFound(new { message = $"Delivery #{id} not found." });

            return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new {message = "An unexpected error occurred while fetching delivery", details = ex.Message });
            }   
        }



        [HttpPut("{id:int}/assign")]
        //[Authorize(Roles = "DeliveryDepartment")]
        public async Task<IActionResult> AssignDelivery(
            int id, [FromBody] AssignDeliveryDto dto)
        {
            try
            {
                var (success, message) = await _deliveryService.AssignDeliveryAsync(id, dto);
                if (!success)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (BusinessException ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while assigning delivery", details = ex.Message });
            }
        }
    }
}
