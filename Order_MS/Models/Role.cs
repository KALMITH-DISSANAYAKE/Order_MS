using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
