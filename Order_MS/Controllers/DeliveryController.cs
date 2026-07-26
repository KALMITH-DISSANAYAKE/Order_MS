using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeliveries()
        {
            var result = await _deliveryService.GetAllDeliveriesAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDeliveryById(int id)
        {
            var result = await _deliveryService.GetDeliveryByIdAsync(id);
            if (result is null)
                return NotFound(new { message = $"Delivery #{id} not found." });

            return Ok(result);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateDeliveryStatus(
            int id, [FromBody] UpdateDeliveryStatusDto dto)
        {
            var (success, message) = await _deliveryService.UpdateDeliveryStatusAsync(id, dto);
            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}