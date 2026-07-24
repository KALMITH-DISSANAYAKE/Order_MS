using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Models;

namespace Order_MS.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly OrderMSDbContext _context;

        public InventoryService(OrderMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Items
                .Include(i => i.Supplier)
                .Select(i => new ProductDto
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    UnitPrice = i.UnitPrice,
                    ReorderLevel = i.ReorderLevel,
                    SupplierName = i.Supplier != null ? i.Supplier.SupplierName : "N/A"
                })
                .ToListAsync();
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
        {
            var item = await _context.Items
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return null;

            return new ProductDetailDto
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                UnitPrice = item.UnitPrice,
                ReorderLevel = item.ReorderLevel,
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
                    ReorderLevel = (int)i.ReorderLevel,
                    IsBelowReorderLevel = i.Quantity < i.ReorderLevel
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LowStockAlertDto>> GetLowStockItemsAsync(int? branchId = null)
        {
            var query = _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Branch)
                .Where(i => i.Quantity < i.ReorderLevel)
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
                    ReorderLevel = i.ReorderLevel
                })
                .ToListAsync();
        }

        public async Task<UpdateStockResponseDto?> UpdateStockAsync(UpdateStockDto dto, int modifiedBy)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.InventoryId == dto.InventoryId);

            if (inventory == null) return null;

            int oldQuantity = inventory.Quantity;
            inventory.Quantity = dto.NewQuantity;
            inventory.ModifiedOn = DateTime.Now;
            // Note: modified_by field doesn't exist on inventory table per schema, 
            // but you can extend schema if needed

            await _context.SaveChangesAsync();

            bool isLow = inventory.Quantity < inventory.ReorderLevel;

            return new UpdateStockResponseDto
            {
                InventoryId = inventory.InventoryId,
                ItemName = inventory.Item?.ItemName ?? "",
                OldQuantity = oldQuantity,
                NewQuantity = inventory.Quantity,
                IsBelowReorderLevel = isLow,
                Message = isLow
                    ? $"Warning: Stock is below reorder level ({inventory.ReorderLevel})!"
                    : "Stock updated successfully."
            };
        }
    }
}