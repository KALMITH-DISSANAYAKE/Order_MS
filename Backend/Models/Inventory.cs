using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int BranchId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public int? ReorderLevel { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;
}
