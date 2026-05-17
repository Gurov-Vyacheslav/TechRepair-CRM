using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class OrderServicePart
{
    public int OrderId { get; set; }

    public int ServiceId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtMoment { get; set; }

    public virtual OrderService OrderService { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
