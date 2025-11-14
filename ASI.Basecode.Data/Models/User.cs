using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class User
{
    /// New numeric auto-increment primary key
    public int Id { get; set; }

    // Retain legacy string UserId for compatibility with mappings and existing code
    public string UserId { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    // Use explicit FirstName/LastName properties instead of Fname/Lname
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingLog> BookingLogs { get; set; } = new List<BookingLog>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<RoomLog> RoomLogs { get; set; } = new List<RoomLog>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
}
