using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwOrderStatusTimestamp
{
    public int? OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? RepairStartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime? CanceledAt { get; set; }

    public DateTime? LastStatusChangedAt { get; set; }
}
