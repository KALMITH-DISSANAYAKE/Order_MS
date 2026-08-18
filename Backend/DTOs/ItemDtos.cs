using System.ComponentModel.DataAnnotations;

namespace Order_MS.DTOs
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool? IsActive { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }

    public class ItemDetailDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool? IsActive { get; set; }
        public SupplierDto Supplier { get; set; } = new();
    }

    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
    }

    public class CreateItemDto
    {
        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 100 characters.")]
        public string ItemName { get; set; } = string.Empty;

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid supplier is required.")]
        public int SupplierId { get; set; }
    }

    public class UpdateItemDto
    {
        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 100 characters.")]
        public string ItemName { get; set; } = string.Empty;

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid supplier is required.")]
        public int SupplierId { get; set; }

        public bool? IsActive { get; set; }
    }
}