using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public string Sku { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int ReorderLevel { get; set; }

    public int? SupplierId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BranchInventory> BranchInventories { get; set; } = new List<BranchInventory>();

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public virtual ICollection<OrderRequestItem> OrderRequestItems { get; set; } = new List<OrderRequestItem>();

    public virtual Supplier? Supplier { get; set; }
}
