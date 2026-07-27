using Order_MS.DTOs;

namespace Order_MS.Services;

public interface IBranchService
{
    // Existing
    Task<List<BranchDto>> GetAllAsync();
    Task<BranchDto?> GetByIdAsync(int id);

    // NEW: Full CRUD
    Task<BranchDto> CreateAsync(CreateBranchDto dto);
    Task<BranchDto?> UpdateAsync(int id, UpdateBranchDto dto);
    Task<bool> DeleteAsync(int id);
}