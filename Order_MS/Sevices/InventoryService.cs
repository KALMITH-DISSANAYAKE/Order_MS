using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;
using Order_MS.Repositories;

namespace Order_MS.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly OrderMSDbContext _context;
        private readonly IGenericRepository<Item> _itemRepo;
        private readonly IGenericRepository<Inventory> _inventoryRepo;

        public InventoryService(
            OrderMSDbContext context,
            IGenericRepository<Item> itemRepo,
            IGenericRepository<Inventory> inventoryRepo)
        {
            _context = context;
            _itemRepo = itemRepo;
            _inventoryRepo = inventoryRepo;
        }

        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
        {
            return await _context.Items
                .Include(i => i.Supplier)
                .Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    UnitPrice = i.UnitPrice,
                    ReorderLevel = i.ReorderLevel ?? 0,  
                    SupplierName = i.Supplier != null ? i.Supplier.SupplierName : "N/A"
                })
                .ToListAsync();
        }

        public async Task<ItemDetailDto?> GetItemByIdAsync(int id)
        {
            var item = await _context.Items
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return null;

            return new ItemDetailDto
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                UnitPrice = item.UnitPrice,
                ReorderLevel = item.ReorderLevel ?? 0,  
                Supplier = item.Supplier == null ? null! : new SupplierDto
                {
                    SupplierId = item.Supplier.SupplierId,
                    SupplierName = item.Supplier.SupplierName,
                    Availability = item.Supplier.Availability ?? "Unknown"
                }
            };
        }

        public async Task<IEnumerable<BranchInventoryDto>> GetBranchInventoryAsync(int branchId)
        {
            var branchExists = await _context.Branches.AnyAsync(b => b.BranchId == branchId);
            if (!branchExists) return new List<BranchInventoryDto>();

            return await _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Branch)
                .Where(i => i.BranchId == branchId)
                .Select(i => new BranchInventoryDto
                {
                    InventoryId = i.InventoryId,
                    BranchId = i.BranchId,
                    BranchCode = i.Branch != null ? i.Branch.BranchCode : "",
                    BranchLocation = i.Branch != null ? i.Branch.Location : "",
                    ItemId = i.ItemId,
                    ItemName = i.Item != null ? i.Item.ItemName : "",
                    Quantity = i.Quantity,
                    ReorderLevel = i.ReorderLevel ?? 0,  
                    IsBelowReorderLevel = (i.Quantity < (i.ReorderLevel ?? 0))  
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LowStockAlertDto>> GetLowStockItemsAsync(int? branchId = null)
        {
            var query = _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Branch)
                .Where(i => i.Quantity < (i.ReorderLevel ?? 0))  
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(i => i.BranchId == branchId.Value);

            return await query
                .Select(i => new LowStockAlertDto
                {
                    InventoryId = i.InventoryId,
                    BranchId = i.BranchId,
                    BranchLocation = i.Branch != null ? i.Branch.Location : "",
                    ItemId = i.ItemId,
                    ItemName = i.Item != null ? i.Item.ItemName : "",
                    CurrentQuantity = i.Quantity,
                    ReorderLevel = i.ReorderLevel ?? 0  
                })
                .ToListAsync();
        }

        public async Task<UpdateStockResponseDto?> UpdateStockAsync(UpdateStockDto dto, int modifiedBy)
        {
            var inventory = await _inventoryRepo.GetByIdAsync(dto.InventoryId) as Inventory;
            if (inventory == null) return null;

            var item = await _itemRepo.GetByIdAsync(inventory.ItemId) as Item;

            int oldQuantity = inventory.Quantity;
            inventory.Quantity = dto.NewQuantity;
            inventory.ModifiedOn = DateTime.Now;

            _inventoryRepo.Update(inventory);
            await _inventoryRepo.SaveAsync();

            int reorderLevel = inventory.ReorderLevel ?? 0;  
            bool isLow = inventory.Quantity < reorderLevel;

            return new UpdateStockResponseDto
            {
                InventoryId = inventory.InventoryId,
                ItemName = item?.ItemName ?? "",
                OldQuantity = oldQuantity,
                NewQuantity = inventory.Quantity,
                IsBelowReorderLevel = isLow,
                Message = isLow
                    ? $"Warning: Stock is below reorder level ({reorderLevel})!"
                    : "Stock updated successfully."
            };
        }
    }
}