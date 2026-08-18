namespace Order_MS.DTOs
{
    public class OrderResponseDtos
    {
        public int OrderId { get; set; }
        public int? OrderReqId { get; set; }
        public int? ConnectionId { get; set; }
        public string? OrderRequestedBy { get; set; }
        public string? OrderBranch { get; set; }
        public decimal? Price { get; set; }
        public string? OrderStatus { get; set; }
        public int? CreatedBy { get; set; }
        public decimal? Total { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? OrderRemark { get; set; }
        public IEnumerable<OrderLineToOrderDtos> OrderLines { get; set; }
        public TransportToOrderDtos? ConnectionLine { get; set; }
    }
}
