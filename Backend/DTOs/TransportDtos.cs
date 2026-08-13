namespace Order_MS.DTOs
{

    public class AvailableDriverVehicleLinkDto
    {
        public int ConnectionId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public int? Capacity { get; set; }
    }

    public class CreateDriverVehicleLinkDto
    {
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
    }


    public class OrderRequestForTransportDto
    {
        public int OrderReqId { get; set; }
        public string ReqStatus { get; set; } = string.Empty;
        public int? TotalQuantity { get; set; }
        public decimal? TotalPrice { get; set; }
        public int BranchId { get; set; }
        public string BranchLocation { get; set; } = string.Empty;
        public DateTime RequestedOn { get; set; }
        public List<OrderRequestLineForTransportDto> Lines { get; set; } = new();
    }

    public class OrderRequestLineForTransportDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    
    public class AssignTransportDto
    {
        public int OrderReqId { get; set; }
        public List<VehicleAssignmentItemDto> Assignments { get; set; } = new();
    }

    public class VehicleAssignmentItemDto
    {

        public int ConnectionId { get; set; }

        public int Quantity { get; set; }
    }


    public class TransportAssignmentResponseDto
    {
        public int AssignmentId { get; set; }
        public int OrderReqId { get; set; }
        public int? ConnectionId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int? VehicleCapacity { get; set; }
        public DateTime AssignedOn { get; set; }
        public string? Status { get; set; }
        public int? Quantity { get; set; }
    }


    public class UpdateAssignmentStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class CreateVehicleDto
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public string Available { get; set; } = "Available";
    }

    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public string Available { get; set; } = string.Empty;
    }

    public class CreateDriverDto
    {
        public string DriversName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Available { get; set; } = "Available";
    }

    public class DriverDto
    {
        public int DriverId { get; set; }
        public string DriversName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Available { get; set; } = string.Empty;
    }
}