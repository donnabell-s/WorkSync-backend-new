using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models;

public partial class RoomLog
{
    public int RoomLogId { get; set; }

    public string RoomIdString { get; set; } // copy of RoomId for audit

    public string RoomName { get; set; } // now persisted

    // Author: numeric user id who made the change
    public int? AuthorId { get; set; } // new audit field

    public string AuthorName { get; set; } // new audit field

    // Change type: create, update, delete, activate, inactivate
    public string ChangeType { get; set; }

    // Descriptive message about the change
    public string Message { get; set; }

    public DateTime? Timestamp { get; set; }
}
