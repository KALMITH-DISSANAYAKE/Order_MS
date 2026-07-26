using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Vehicle
{
    public int VehicalId { get; set; }

    public string VehicalNumber { get; set; } = null!;

    public string? Available { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<DriverVehicleLink> DriverVehicleLinks { get; set; } = new List<DriverVehicleLink>();

    public virtual ICollection<TransportAssignment> TransportAssignments { get; set; } = new List<TransportAssignment>();
}
