using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Session
{
    public string SessionId { get; set; }

    public bool? Auth { get; set; }

    public int? UserRefId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; }
}
