using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order_MS.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int? ReorderLevel { get; set; }

    public int? SupplierId { get; set; }

    public int? CreatedBy { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public virtual ICollection<OrderRequestLine> OrderRequestLines { get; set; } = new List<OrderRequestLine>();

    public virtual Supplier? Supplier { get; set; }
}
