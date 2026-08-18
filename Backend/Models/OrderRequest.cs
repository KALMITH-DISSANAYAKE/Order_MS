using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class OrderRequest
{
    public int OrderReqId { get; set; }

    public string? ReqStatus { get; set; }

    public int? TotalQuantity { get; set; }

    public decimal? TotalPrice { get; set; }

    public int RequestedBy { get; set; }

    public DateTime? RequestedOn { get; set; }

    public int BranchId { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int? ReceivedBy { get; set; }

    public DateTime? ReceivedOn { get; set; }

    public string? OrderReqRemark { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<OrderRequestLine> OrderRequestLines { get; set; } = new List<OrderRequestLine>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User RequestedByNavigation { get; set; } = null!;

    public virtual ICollection<TransportAssignment> TransportAssignments { get; set; } = new List<TransportAssignment>();
}
