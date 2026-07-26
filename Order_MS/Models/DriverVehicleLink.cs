using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class DriverVehicleLink
{
    public int ConnectionId { get; set; }

    public int DriverId { get; set; }

    public int VehicalId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Vehicle Vehical { get; set; } = null!;
}
