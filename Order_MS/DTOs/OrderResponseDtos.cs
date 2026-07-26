namespace Order_MS.DTOs
{
    public class OrderResponseDtos
    {
        public int? OrderReqId { get; set; }
        public int? ConnectionId { get; set; }
        public decimal? Price { get; set; }
        public string? OrderStatus { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? OrderRemark { get; set; }
        public List<OrderLineResponse> OrderLines { get; set; } = new List<OrderLineResponse>();
    }
}
