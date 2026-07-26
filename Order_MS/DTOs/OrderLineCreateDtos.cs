using System.ComponentModel.DataAnnotations;
using Order_MS.Models;

namespace Order_MS.DTOs
{
    public class OrderLineCreateDtos
    {
        [Required]
        public int OrderlineId { get; set; }

        [Required]
        public int OrderId { get; set; }
        [Required]
        public int ItemId { get; set; }
        [Required]
        public int? SupplierId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal? UnitPrice { get; set; }
        [Required]
        public decimal? TotalPrice { get; set; }

        public virtual Item Item { get; set; } = null!;

        public virtual Order Order { get; set; } = null!;

        public virtual Supplier? Supplier { get; set; }
    }
}
