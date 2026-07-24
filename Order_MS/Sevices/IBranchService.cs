using Order_MS.DTOs;

namespace Order_MS.Services;

public interface IBranchService
{
    // Get all branches
    Task<List<BranchDto>> GetAllAsync();

    // Get one branch by ID (returns null if not found)
    Task<BranchDto?> GetByIdAsync(int id);
}