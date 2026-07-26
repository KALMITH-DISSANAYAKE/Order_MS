using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class BranchInventory
{
    public int InventoryId { get; set; }

    public int BranchId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
