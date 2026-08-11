using Order_MS.Models;
namespace Order_MS.DTOs
{
    public class OrderLineResponse
    {
        public int OrderlineId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int? SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
