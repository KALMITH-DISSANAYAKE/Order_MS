<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Driver
{
    public int DriverId { get; set; }

    public string DriversName { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public string? Available { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<DriverVehicleLink> DriverVehicleLinks { get; set; } = new List<DriverVehicleLink>();

    public virtual ICollection<TransportAssignment> TransportAssignments { get; set; } = new List<TransportAssignment>();
}
=======
﻿using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Driver
{
    public int DriverId { get; set; }

    public string DriversName { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public string? Available { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<DriverVehicleLink> DriverVehicleLinks { get; set; } = new List<DriverVehicleLink>();

    public virtual ICollection<TransportAssignment> TransportAssignments { get; set; } = new List<TransportAssignment>();
}
>>>>>>> order-requests
