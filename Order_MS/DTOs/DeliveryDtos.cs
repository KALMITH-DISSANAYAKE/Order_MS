namespace Order_MS.DTOs
{
    public class DeliveryListDto
    {
        public int OrderId { get; set; }
        public int? OrderReqId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? DriverName { get; set; }
        public string? VehicleNumber { get; set; }
        public string? BranchLocation { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class DeliveryDetailDto : DeliveryListDto
    {
        public string? OrderRemark { get; set; }
        public List<DeliveryLineDto> Lines { get; set; } = new();
    }

    public class DeliveryLineDto
    {
        public int OrderLineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}