namespace Order_MS.DTOs
{
    public class VerifyDeliveryDto
    {
        public string OrderNumber { get; set; }
        public string VehicleNumber { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
    }

    public class DeliveryResponseDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverLicense { get; set; }
        public DateTime? DeliveredOn { get; set; }
    }

    public class DeliveryHistoryDto
    {
        public int OrderId { get; set; }
        public int? BranchId { get; set; }
        public string OrderStatus { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverLicense { get; set; }
        public DateTime? DeliveredOn { get; set; }
    }
}