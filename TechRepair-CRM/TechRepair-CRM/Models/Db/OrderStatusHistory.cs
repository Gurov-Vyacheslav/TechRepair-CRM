using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class OrderStatusHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public short StatusId { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Comment { get; set; }

    public virtual RepairOrder Order { get; set; } = null!;

    public virtual OrderStatus Status { get; set; } = null!;
}
