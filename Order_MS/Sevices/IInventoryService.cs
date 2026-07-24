using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDetailDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<BranchInventoryDto>> GetBranchInventoryAsync(int branchId);
        Task<IEnumerable<LowStockAlertDto>> GetLowStockItemsAsync(int? branchId = null);
        Task<UpdateStockResponseDto?> UpdateStockAsync(UpdateStockDto dto, int modifiedBy);
    }
}