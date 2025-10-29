using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class RoomAmenity
{
    public string RoomId { get; set; }

    public string Amenity { get; set; }

    public virtual Room Room { get; set; }
}
