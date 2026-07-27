namespace Order_MS.DTOs
{
    public class TransportToOrderDtos
    {
        public int DriverId { get; set; }
        public string DriversName { get; set; } = null!;
        public string DriverLicenseNumber { get; set; } = null!;
        public int VehicalId { get; set; }
        public string VehicalNumber { get; set; }
    }
}
