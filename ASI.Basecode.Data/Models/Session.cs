using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Session
{
    public string SessionId { get; set; }

    public bool? Auth { get; set; }

    public string UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; }
}
