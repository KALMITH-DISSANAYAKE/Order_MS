using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class OrderLine
{
    public int OrderlineId { get; set; }

    public int OrderId { get; set; }

    public int ItemId { get; set; }

    public int? SupplierId { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Supplier? Supplier { get; set; }
}
