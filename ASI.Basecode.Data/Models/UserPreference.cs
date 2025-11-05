using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class UserPreference
{
    public int PrefId { get; set; }

    public int? UserRefId { get; set; }

    public bool? BookingEmailConfirm { get; set; }

    public bool? CancellationNotif { get; set; }

    public bool? BookingReminder { get; set; }

    public int? ReminderTimeMinutes { get; set; }

    public int? BookingDefaultMinutes { get; set; }

    public string RawJson { get; set; }

    public virtual User User { get; set; }
}
