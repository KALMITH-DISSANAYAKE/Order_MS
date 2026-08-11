using System.ComponentModel.DataAnnotations;

namespace Order_MS.DTOs
{
    public class OrderLineCreateDtos
    {
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
    }
}
