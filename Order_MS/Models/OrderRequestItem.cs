using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class OrderRequestItem
{
    public int OrderRequestItemId { get; set; }

    public int OrderRequestId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual OrderRequest OrderRequest { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
