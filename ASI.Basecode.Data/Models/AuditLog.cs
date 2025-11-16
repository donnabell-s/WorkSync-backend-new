using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public int? UserRefId { get; set; }

    public string Action { get; set; }

    public string EntityType { get; set; }

    public int? EntityId { get; set; }

    public string OldValues { get; set; }

    public string NewValues { get; set; }

    public string IpAddress { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual User User { get; set; }
}

