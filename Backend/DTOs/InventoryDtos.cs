namespace Order_MS.DTOs
{
    public class BranchInventoryDto
    {
        public int InventoryId { get; set; }
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsBelowReorderLevel { get; set; }
    }

    public class LowStockAlertDto
    {
        public int InventoryId { get; set; }
        public int BranchId { get; set; }
        public string BranchLocation { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public int ShortageAmount => ReorderLevel - CurrentQuantity;
    }

    public class UpdateStockDto
    {
        public int InventoryId { get; set; }
        public int NewQuantity { get; set; }
        public int? ReorderLevel { get; set; }
    }

    public class UpdateStockResponseDto
    {
        public int InventoryId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public bool IsBelowReorderLevel { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AddBranchInventoryDto
    {
        public int BranchId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }
}