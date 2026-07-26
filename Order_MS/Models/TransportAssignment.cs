using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class TransportAssignment
{
    public int AssignmentId { get; set; }

    public int OrderReqId { get; set; }

    public int? ConnectionId { get; set; }

    public DateTime AssignedOn { get; set; }

    public string? Status { get; set; }

    public int? Quantity { get; set; }

    public virtual DriverVehicleLink? Connection { get; set; }

    public virtual OrderRequest OrderReq { get; set; } = null!;
}
