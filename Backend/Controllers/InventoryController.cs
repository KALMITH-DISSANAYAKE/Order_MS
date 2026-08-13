using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_MS.DTOs;
using Order_MS.Services;
using System.Security.Claims;

namespace Order_MS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _inventoryService.GetAllItemsAsync();
            return Ok(items);
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _inventoryService.GetItemByIdAsync(id);
            return Ok(item);
        }

        [HttpPost("items")]
       // [Authorize(Roles = "InventoryManager")]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? createdBy = int.TryParse(userIdClaim, out int uid) && uid > 0 ? uid : null;

            var createdItem = await _inventoryService.CreateItemAsync(dto, createdBy);
            return CreatedAtAction(nameof(GetItemById), new { id = createdItem.ItemId }, createdItem);
        }

        [HttpPut("items/{id}")]
       // [Authorize(Roles = "InventoryManager")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? modifiedBy = int.TryParse(userIdClaim, out int uid) && uid > 0 ? uid : null;

            var updatedItem = await _inventoryService.UpdateItemAsync(id, dto, modifiedBy);
            return Ok(updatedItem);
        }

        [HttpDelete("items/{id}")]
       // [Authorize(Roles = "InventoryManager")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            await _inventoryService.DeleteItemAsync(id);
            return NoContent();
        }

       [HttpGet("branch")]
        public async Task<IActionResult> GetAllBranchInventory()
        {
            var inventory = await _inventoryService.GetAllBranchInventoryAsync();
            return Ok(inventory);
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetBranchInventory(int branchId)
        {
            var inventory = await _inventoryService.GetBranchInventoryAsync(branchId);
            if (!inventory.Any())
                return NotFound(new { message = "No inventory found for this branch" });

            return Ok(inventory);
        }

        [HttpGet("low-stock")]
        //[Authorize(Roles = "BranchManager,InventoryManager")]
        public async Task<IActionResult> GetLowStock([FromQuery] int? branchId = null)
        {
            var lowStock = await _inventoryService.GetLowStockItemsAsync(branchId);
            return Ok(lowStock);
        }

        [HttpPut("update")]
        [Authorize(Roles = "BranchManager,InventoryManager")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? modifiedBy = int.TryParse(userIdClaim, out int uid) && uid > 0 ? uid : null;

            var result = await _inventoryService.UpdateStockAsync(dto, modifiedBy);
            return Ok(result);
        }
    }
}