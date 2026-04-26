using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class OrderStatus
{
    public short StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
}
