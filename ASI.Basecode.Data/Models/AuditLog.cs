using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string UserId { get; set; }

    public string Action { get; set; }

    public string EntityType { get; set; }

    public string EntityId { get; set; }

    public string Details { get; set; }

    public string IpAddress { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual User User { get; set; }
}

