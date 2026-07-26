using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class OrderRequestLine
{
    public int OrderReqLineId { get; set; }

    public int OrderReqId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual OrderRequest OrderReq { get; set; } = null!;
}
