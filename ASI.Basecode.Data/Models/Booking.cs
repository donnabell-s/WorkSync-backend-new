using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public string RoomId { get; set; }

    // switched to numeric FK column
    public int? UserRefId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime? StartDatetime { get; set; }

    public DateTime? EndDatetime { get; set; }

    public string Recurrence { get; set; }

    public string Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingLog> BookingLogs { get; set; } = new List<BookingLog>();

    public virtual Room Room { get; set; }

    public virtual User User { get; set; }
}
