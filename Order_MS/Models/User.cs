using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public int? BranchId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Branch> BranchCreatedByNavigations { get; set; } = new List<Branch>();

    public virtual ICollection<Branch> BranchModifiedByNavigations { get; set; } = new List<Branch>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<User> InverseModifiedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<Item> ItemCreatedByNavigations { get; set; } = new List<Item>();

    public virtual ICollection<Item> ItemModifiedByNavigations { get; set; } = new List<Item>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Order> OrderCreatedByNavigations { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderModifiedByNavigations { get; set; } = new List<Order>();

    public virtual ICollection<OrderRequest> OrderRequestApprovedByNavigations { get; set; } = new List<OrderRequest>();

    public virtual ICollection<OrderRequest> OrderRequestModifiedByNavigations { get; set; } = new List<OrderRequest>();

    public virtual ICollection<OrderRequest> OrderRequestRequestedByNavigations { get; set; } = new List<OrderRequest>();

    public virtual Role Role { get; set; } = null!;
}
