using Order_MS.DTOs;

namespace Order_MS.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();
        Task<ItemDetailDto?> GetItemByIdAsync(int id);         
        Task<IEnumerable<BranchInventoryDto>> GetAllBranchInventoryAsync();
        Task<IEnumerable<BranchInventoryDto>> GetBranchInventoryAsync(int branchId);
        Task<IEnumerable<LowStockAlertDto>> GetLowStockItemsAsync(int? branchId = null);
        Task<UpdateStockResponseDto> UpdateStockAsync(UpdateStockDto dto, int? modifiedBy); 
        Task DeleteBranchStockAsync(int inventoryId);
        Task<BranchInventoryDto> AddBranchInventoryAsync(AddBranchInventoryDto dto, int? createdBy);
        Task<ItemDetailDto> CreateItemAsync(CreateItemDto dto, int? createdBy);
        Task<ItemDetailDto> UpdateItemAsync(int id, UpdateItemDto dto, int? modifiedBy);   
        Task DeleteItemAsync(int id);
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
    }
}
