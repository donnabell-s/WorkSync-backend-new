using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Room
{
    public string RoomId { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public int? Seats { get; set; }

    public string Location { get; set; }

    public string Level { get; set; }

    public string SizeLabel { get; set; }

    public string Status { get; set; }

    public string OperatingHours { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();

    public virtual ICollection<RoomLog> RoomLogs { get; set; } = new List<RoomLog>();
}
