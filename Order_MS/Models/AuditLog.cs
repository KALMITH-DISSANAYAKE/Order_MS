using System;
using System.Collections.Generic;

namespace Order_MS.Models;

public partial class AuditLog
{
    public int AuditId { get; set; }

    public string? TableName { get; set; }

    public string? Action { get; set; }

    public int? RecordId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public int? PerformedBy { get; set; }

    public DateTime? PerformedAt { get; set; }

    public virtual User? PerformedByNavigation { get; set; }
}
