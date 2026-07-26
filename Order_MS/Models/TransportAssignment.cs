<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class TransportAssignment
{
    public int AssignmentId { get; set; }

    public int OrderId { get; set; }

    public int VehicleId { get; set; }

    public int DriverId { get; set; }

    public DateTime AssignedOn { get; set; }

    public string? Status { get; set; }

    public int? Quantity { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
=======
﻿using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class TransportAssignment
{
    public int AssignmentId { get; set; }

    public int OrderReqId { get; set; }

    public int? VehicleId { get; set; }

    public int? DriverId { get; set; }

    public DateTime? AssignedOn { get; set; }

    public string? Status { get; set; }

    public int? Quantity { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual OrderRequest OrderReq { get; set; } = null!;

    public virtual Vehicle? Vehicle { get; set; }
}
>>>>>>> order-requests
