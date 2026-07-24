namespace Order_MS.DTOs
{
    public class AssignTransportDto
    {
        public int OrderId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
    }

    public class TransportResponseDto
    {
        public int AssignmentId { get; set; }
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; }
        public int DriverId { get; set; }
        public string DriverLicense { get; set; }
        public DateTime AssignedOn { get; set; }
        public string Status { get; set; }
    }

    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; }
        public bool Available { get; set; }
    }

    public class DriverDto
    {
        public int DriverId { get; set; }
        public string LicenseNumber { get; set; }
        public bool Available { get; set; }
    }
}