using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Branch
{
    public int BranchId { get; set; }

    public string BranchCode { get; set; } = null!;

    public string Location { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<OrderRequest> OrderRequests { get; set; } = new List<OrderRequest>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
