namespace Order_MS.DTOs
{
    public class OrderLineToOrderDtos
    {
        public int OrderlineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
