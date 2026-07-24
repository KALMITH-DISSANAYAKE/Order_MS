using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class DeliveryVerification
{
    public int VerificationId { get; set; }

    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string VehicleNumber { get; set; } = null!;

    public int VerifiedBy { get; set; }

    public DateTime? VerificationDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User VerifiedByNavigation { get; set; } = null!;
}
