using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class DriverVehicleLink
{
    public int ConnectionId { get; set; }

    public int DriverId { get; set; }

    public int VehicleId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? Status { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<TransportAssignment> TransportAssignments { get; set; } = new List<TransportAssignment>();

    public virtual Vehicle Vehicle { get; set; } = null!;
}
