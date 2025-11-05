using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string UserId { get; set; }

    public string Title { get; set; }

    public string Message { get; set; }

    public string Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual User User { get; set; }
}

