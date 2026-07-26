using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;
using System.Security.Claims;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: api/inventory/items
        [HttpGet("items")]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _inventoryService.GetAllItemsAsync();
            return Ok(items);
        }

        // GET: api/inventory/items/1
        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _inventoryService.GetItemByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found" });

            return Ok(item);
        }

        // GET: api/inventory/branch/1
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetBranchInventory(int branchId)
        {
            var inventory = await _inventoryService.GetBranchInventoryAsync(branchId);
            if (!inventory.Any())
                return NotFound(new { message = "No inventory found for this branch" });

            return Ok(inventory);
        }

        // GET: api/inventory/low-stock?branchId=1
        [HttpGet("low-stock")]
        [Authorize(Roles = "BranchManager,InventoryManager")]
        public async Task<IActionResult> GetLowStock([FromQuery] int? branchId = null)
        {
            var lowStock = await _inventoryService.GetLowStockItemsAsync(branchId);
            return Ok(lowStock);
        }

        // PUT: api/inventory/update
        [HttpPut("update")]
        [Authorize(Roles = "BranchManager,InventoryManager")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int modifiedBy = int.TryParse(userIdClaim, out int uid) ? uid : 0;

            var result = await _inventoryService.UpdateStockAsync(dto, modifiedBy);
            if (result == null)
                return NotFound(new { message = "Inventory record not found" });

            return Ok(result);
        }
    }
}