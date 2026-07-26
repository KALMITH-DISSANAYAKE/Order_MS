using Order_MS.DTOs;
using Order_MS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Order_MS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    // GET /api/branches
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetAllAsync();
        return Ok(branches);
    }

    // GET /api/branches/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch == null)
            return NotFound(new { message = "Branch not found" });

        return Ok(branch);
    }

    // POST /api/branches
    [HttpPost]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        var created = await _branchService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.BranchId }, created);
    }

    // PUT /api/branches/1
    [HttpPut("{id}")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
    {
        var updated = await _branchService.UpdateAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = "Branch not found" });

        return Ok(updated);
    }

    // DELETE /api/branches/1
    [HttpDelete("{id}")]
    [Authorize(Roles = "InventoryManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _branchService.DeleteAsync(id);
        if (!success)
            return NotFound(new { message = "Branch not found" });

        return NoContent(); // 204 = success, no body needed
    }
}