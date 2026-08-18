using System.ComponentModel.DataAnnotations;

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
        [Range(1, int.MaxValue, ErrorMessage = "Valid Inventory ID is required.")]
        public int InventoryId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int NewQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
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
        [Range(1, int.MaxValue, ErrorMessage = "Valid Branch ID is required.")]
        public int BranchId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Valid Item ID is required.")]
        public int ItemId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
        public int ReorderLevel { get; set; }
    }
}