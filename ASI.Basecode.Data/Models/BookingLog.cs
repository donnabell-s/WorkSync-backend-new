using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class BookingLog
{
    public int BookingLogId { get; set; }
    public string BookingIdString { get; set; } // copy of BookingId for audit
    public string BookingName { get; set; } // name/title of booking
    public int? AuthorId { get; set; } // user id who made the change
    public string AuthorName { get; set; } // user name who made the change
    public string ChangeType { get; set; }
    public string Message { get; set; }
    public DateTime? Timestamp { get; set; }
}
