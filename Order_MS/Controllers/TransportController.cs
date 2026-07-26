using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService)
        {
            _transportService = transportService;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignTransport([FromBody] AssignTransportDto dto)
        {
            try
            {
                var result = await _transportService.AssignTransportAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vehicles/available")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            var vehicles = await _transportService.GetAvailableVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("drivers/available")]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            var drivers = await _transportService.GetAvailableDriversAsync();
            return Ok(drivers);
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments()
        {
            var assignments = await _transportService.GetAssignmentsAsync();
            return Ok(assignments);
        }

        [HttpGet("assignments/{orderId}")]
        public async Task<IActionResult> GetAssignmentsByOrderId(int orderId)
        {
            var assignments = await _transportService.GetAssignmentsByOrderIdAsync(orderId);
            return Ok(assignments);
        }
    }
}