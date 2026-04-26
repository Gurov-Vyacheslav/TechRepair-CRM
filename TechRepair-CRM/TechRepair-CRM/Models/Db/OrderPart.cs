using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class OrderPart
{
    public int OrderId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtMoment { get; set; }

    public virtual RepairOrder Order { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
