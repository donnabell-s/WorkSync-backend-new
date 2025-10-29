using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class BookingLog
{
    public int BookingLogId { get; set; }

    public int? BookingId { get; set; }

    public string UserId { get; set; }

    public string EventType { get; set; }

    public string CurrentStatus { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Booking Booking { get; set; }

    public virtual User User { get; set; }
}
