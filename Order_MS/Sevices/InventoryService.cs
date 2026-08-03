using Microsoft.EntityFrameworkCore;
using Order_MS.Data;
using Order_MS.DTOs;
using Order_MS.Exceptions; 
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
                .Where(i => i.IsActive != false)
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
                .Where(i => i.IsActive != false)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
                throw new BusinessException($"Item with ID {id} not found.", 404);

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
            if (!branchExists)
                throw new BusinessException($"Branch with ID {branchId} does not exist.", 404);

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
                    IsBelowReorderLevel = i.Quantity < (i.ReorderLevel ?? 0)
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

        public async Task<UpdateStockResponseDto> UpdateStockAsync(UpdateStockDto dto, int? modifiedBy)
        {

            if (dto.InventoryId <= 0)
                throw new BusinessException("Inventory ID must be greater than 0.", 400);

            if (dto.NewQuantity < 0)
                throw new BusinessException("Quantity cannot be negative.", 400);


            var inventory = await _inventoryRepo.GetByIdAsync(dto.InventoryId) as Inventory;
            if (inventory == null)
                throw new BusinessException($"Inventory record with ID {dto.InventoryId} not found.", 404);

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

        public async Task<ItemDetailDto> CreateItemAsync(CreateItemDto dto, int? createdBy)
        {

            if (dto.UnitPrice <= 0)
                throw new BusinessException("Unit price must be greater than 0.", 400);

            if (dto.ReorderLevel < 0)
                throw new BusinessException("Reorder level cannot be negative.", 400);

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.SupplierId == dto.SupplierId);
            if (!supplierExists)
                throw new BusinessException($"Supplier with ID {dto.SupplierId} does not exist.", 400);

            var item = new Item
            {
                ItemName = dto.ItemName,
                UnitPrice = dto.UnitPrice,
                ReorderLevel = dto.ReorderLevel,
                SupplierId = dto.SupplierId,
                IsActive = true,
                CreatedOn = DateTime.Now
            };

            if (createdBy.HasValue && createdBy.Value > 0)
                item.CreatedBy = createdBy.Value;

            try
            {
                await _itemRepo.AddAsync(item);
                await _itemRepo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException($"Database error: {ex.InnerException?.Message ?? ex.Message}", 400);
            }

            var result = await GetItemByIdAsync(item.ItemId);
            if (result == null)
                throw new BusinessException("Item was created but could not be retrieved.", 500);

            return result;
        }

        public async Task<ItemDetailDto> UpdateItemAsync(int id, UpdateItemDto dto, int? modifiedBy)
        {
            if (id <= 0)
                throw new BusinessException("Item ID must be greater than 0.", 400);

            if (dto.UnitPrice <= 0)
                throw new BusinessException("Unit price must be greater than 0.", 400);

            if (dto.ReorderLevel < 0)
                throw new BusinessException("Reorder level cannot be negative.", 400);

            var item = await _itemRepo.GetByIdAsync(id) as Item;
            if (item == null || item.IsActive == false)
                throw new BusinessException($"Item with ID {id} not found or has been deleted.", 404);

            var supplierExists = await _context.Suppliers.AnyAsync(s => s.SupplierId == dto.SupplierId);
            if (!supplierExists)
                throw new BusinessException($"Supplier with ID {dto.SupplierId} does not exist.", 400);

            item.ItemName = dto.ItemName;
            item.UnitPrice = dto.UnitPrice;
            item.ReorderLevel = dto.ReorderLevel;
            item.SupplierId = dto.SupplierId;
            item.ModifiedOn = DateTime.Now;

            if (modifiedBy.HasValue && modifiedBy.Value > 0)
                item.ModifiedBy = modifiedBy.Value;

            try
            {
                _itemRepo.Update(item);
                await _itemRepo.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new BusinessException($"Database error: {ex.InnerException?.Message ?? ex.Message}", 400);
            }

            return await GetItemByIdAsync(id);
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await _itemRepo.GetByIdAsync(id) as Item;
            if (item == null || item.IsActive == false)
                throw new BusinessException($"Item with ID {id} not found or already deleted.", 404);

            bool hasInventory = await _context.Inventories.AnyAsync(i => i.ItemId == id);
            if (hasInventory)
                throw new BusinessException("Cannot delete item that exists in branch inventory. Remove stock from all branches first.", 400);

            item.IsActive = false;
            item.ModifiedOn = DateTime.Now;

            _itemRepo.Update(item);
            await _itemRepo.SaveAsync();
        }
    }
}