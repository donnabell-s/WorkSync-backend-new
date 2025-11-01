using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class RoomLog
{
    public int RoomLogId { get; set; }

    public string RoomId { get; set; }

    public int? UserRefId { get; set; }

    public string EventType { get; set; }

    public string CurrentStatus { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Room Room { get; set; }

    public virtual User User { get; set; }
}
