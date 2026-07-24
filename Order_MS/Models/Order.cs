using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? OrderReqId { get; set; }

    public int? ConnectionId { get; set; }

    public decimal? Price { get; set; }

    public string? OrderStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual DriverVehicleLink? Connection { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public virtual OrderRequest? OrderReq { get; set; }
}
