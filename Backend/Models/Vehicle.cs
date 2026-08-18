using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string VehicleNumber { get; set; } = null!;

    public string? Available { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? Capacity { get; set; }

    public virtual ICollection<DriverVehicleLink> DriverVehicleLinks { get; set; } = new List<DriverVehicleLink>();
}
